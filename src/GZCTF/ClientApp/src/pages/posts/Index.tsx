import { Button, Group, Pagination, Stack, Title } from '@mantine/core'
import { mdiPlus } from '@mdi/js'
import { Icon } from '@mdi/react'
import { FC, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { Empty } from '@Components/Empty'
import { PostCard } from '@Components/PostCard'
import { WithNavBar } from '@Components/WithNavbar'
import { RequireRole } from '@Components/WithRole'
import { showErrorMsg } from '@Utils/Shared'
import { OnceSWRConfig } from '@Hooks/useConfig'
import { usePageTitle } from '@Hooks/usePageTitle'
import { useUserRole } from '@Hooks/useUser'
import api, { PostInfoModel, Role } from '@Api'
import experience from '@Styles/Experience.module.css'
import misc from '@Styles/Misc.module.css'

const ITEMS_PER_PAGE = 10

const Posts: FC = () => {
  const { data: posts, mutate } = api.info.useInfoGetPosts(OnceSWRConfig)

  const [activePage, setPage] = useState(1)
  const { role } = useUserRole()

  const { t } = useTranslation()

  usePageTitle(t('post.title.index'))

  const onTogglePinned = async (post: PostInfoModel, setDisabled: (value: boolean) => void) => {
    setDisabled(true)

    try {
      const res = await api.edit.editUpdatePost(post.id, {
        isPinned: !post.isPinned,
      })
      if (post.isPinned) {
        mutate([
          ...(posts?.filter((p) => p.id !== post.id && p.isPinned) ?? []),
          { ...res.data },
          ...(posts?.filter((p) => p.id !== post.id && !p.isPinned) ?? []),
        ])
      } else {
        mutate([
          { ...res.data },
          ...(posts?.filter((p) => p.id !== post.id && p.isPinned) ?? []),
          ...(posts?.filter((p) => p.id !== post.id && !p.isPinned) ?? []),
        ])
      }
      api.info.mutateInfoGetLatestPosts()
    } catch (e) {
      showErrorMsg(e, t)
    } finally {
      setDisabled(false)
    }
  }

  return (
    <WithNavBar isLoading={!posts} minWidth={0} withHeader stickyHeader>
      <Stack className={`${experience.page} ${experience.pageStack}`} justify="space-between" mih="calc(100vh - 78px)">
        <Stack className={experience.list}>
          <div className={experience.pageHeader}>
            <Stack className={experience.headingGroup}>
              <Title order={1} className={experience.pageTitle}>
                {t('post.title.index')}
              </Title>
            </Stack>
          </div>
          {posts && posts.length === 0 && <Empty bordered />}
          {posts?.slice((activePage - 1) * ITEMS_PER_PAGE, activePage * ITEMS_PER_PAGE).map((post) => (
            <PostCard key={post.id} post={post} onTogglePinned={onTogglePinned} />
          ))}
        </Stack>

        <Pagination.Root
          total={Math.ceil((posts?.length ?? 0) / ITEMS_PER_PAGE)}
          siblings={3}
          value={activePage}
          onChange={setPage}
          mb="xl"
        >
          <Group className={experience.pagination} gap={5} justify="flex-end">
            <Pagination.First />
            <Pagination.Previous />
            <Pagination.Items />
            <Pagination.Next />
            <Pagination.Last />
          </Group>
        </Pagination.Root>
      </Stack>
      {RequireRole(Role.Admin, role) && (
        <Button
          component={Link}
          className={misc.fixedButton}
          __vars={{
            '--fixed-right': 'calc(0.1 * (100vw - 70px - 2rem) + 1rem)',
            '--fixed-bottom': '6rem',
          }}
          variant="filled"
          size="md"
          leftSection={<Icon path={mdiPlus} size={1} />}
          to="/posts/new/edit"
        >
          {t('post.button.new')}
        </Button>
      )}
    </WithNavBar>
  )
}

export default Posts
