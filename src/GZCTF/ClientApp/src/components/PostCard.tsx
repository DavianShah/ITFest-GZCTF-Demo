import { ActionIcon, Anchor, Avatar, Card, Group, Stack, Text, Title } from '@mantine/core'
import { mdiFormatQuoteOpen, mdiPencilOutline, mdiPinOffOutline, mdiPinOutline } from '@mdi/js'
import { Icon } from '@mdi/react'
import dayjs from 'dayjs'
import { FC, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { Markdown } from '@Components/MarkdownRenderer'
import { RequireRole } from '@Components/WithRole'
import { useLanguage } from '@Utils/I18n'
import { useUserRole } from '@Hooks/useUser'
import { PostInfoModel, Role } from '@Api'
import classes from '@Styles/PostCard.module.css'

export interface PostCardProps {
  post: PostInfoModel
  onTogglePinned?: (post: PostInfoModel, setDisabled: (value: boolean) => void) => void
}

export const PostCard: FC<PostCardProps> = ({ post, onTogglePinned }) => {
  const { role } = useUserRole()
  const { t } = useTranslation()
  const [disabled, setDisabled] = useState(false)

  const { locale } = useLanguage()

  return (
    <Card className={classes.root} data-pinned={post.isPinned || undefined} radius={0} p="md">
      <Group className={classes.layout} wrap="nowrap" justify="space-between" align="flex-start">
        <Icon className={classes.quote} path={mdiFormatQuoteOpen} size={1.5} />
        <Stack className={classes.body} gap="xs" w="calc(100% - 3rem)">
          {RequireRole(Role.Admin, role) ? (
            <Group className={classes.header} justify="space-between" wrap="nowrap">
              <Title order={3} className={classes.title}>
                {post.isPinned && (
                  <Text className={classes.pinned} fw="bold" span>
                    {t('post.content.pinned')}&nbsp;&nbsp;
                  </Text>
                )}
                {post.title}
              </Title>
              <Group className={classes.controls} justify="right">
                {onTogglePinned && (
                  <ActionIcon disabled={disabled} onClick={() => onTogglePinned(post, setDisabled)}>
                    {post.isPinned ? <Icon path={mdiPinOffOutline} size={1} /> : <Icon path={mdiPinOutline} size={1} />}
                  </ActionIcon>
                )}
                <ActionIcon component={Link} to={`/posts/${post.id}/edit`}>
                  <Icon path={mdiPencilOutline} size={1} />
                </ActionIcon>
              </Group>
            </Group>
          ) : (
            <Title order={3} className={classes.title}>
              {post.isPinned && (
                <Text className={classes.pinned} fw="bold" span>
                  {`${t('post.content.pinned')} `}
                </Text>
              )}
              {post.title}
            </Title>
          )}
          <Markdown className={classes.summary} source={post.summary} />
          {post.tags && (
            <Group className={classes.tags}>
              {post.tags.map((tag, idx) => (
                <Text key={idx} className={classes.tag} size="sm" fw="bold" span>
                  {`#${tag}`}
                </Text>
              ))}
            </Group>
          )}
          <Group className={classes.footer} w="100%" justify="space-between" m="auto" fs="normal">
            <Group className={classes.author} gap={5} justify="right" wrap="nowrap">
              <Avatar alt="avatar" src={post.authorAvatar} size="sm">
                {post.authorName?.slice(0, 1) ?? 'A'}
              </Avatar>
              <Text className={classes.metadata} size="sm" fw="bold">
                {t('post.content.metadata', {
                  author: post.authorName ?? 'Anonym',
                  date: dayjs(post.time).locale(locale).format('LLL'),
                })}
              </Text>
            </Group>
            <Text ta="right">
              <Anchor component={Link} to={`/posts/${post.id}`}>
                <Text className={classes.details} span fw="bold" size="sm">
                  {t('post.content.details')} &gt;&gt;&gt;
                </Text>
              </Anchor>
            </Text>
          </Group>
        </Stack>
      </Group>
    </Card>
  )
}
