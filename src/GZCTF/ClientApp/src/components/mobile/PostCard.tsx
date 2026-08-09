import { ActionIcon, Avatar, Box, Card, Group, Stack, Text, Title } from '@mantine/core'
import { mdiPencilOutline, mdiPinOffOutline, mdiPinOutline } from '@mdi/js'
import { Icon } from '@mdi/react'
import dayjs from 'dayjs'
import { FC, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router'
import { Markdown } from '@Components/MarkdownRenderer'
import { PostCardProps } from '@Components/PostCard'
import { RequireRole } from '@Components/WithRole'
import { useUserRole } from '@Hooks/useUser'
import { Role } from '@Api'
import classes from '@Styles/PostCard.module.css'

export const MobilePostCard: FC<PostCardProps> = ({ post, onTogglePinned }) => {
  const { role } = useUserRole()
  const [disabled, setDisabled] = useState(false)

  const { t } = useTranslation()
  const navigate = useNavigate()
  return (
    <Card
      className={`${classes.root} ${classes.mobileRoot}`}
      data-pinned={post.isPinned || undefined}
      radius={0}
      p="sm"
    >
      <Stack gap="xs">
        <Box className={classes.mobileTarget} onClick={() => navigate(`/posts/${post.id}`)}>
          <Title order={3} className={classes.title} pb={4}>
            <Text className={classes.pinned} fw="bold" span>
              {post.isPinned ? `${t('post.content.pinned')} ` : '>>> '}
            </Text>
            {post.title}
          </Title>
          <Markdown className={classes.summary} source={post.summary} />
        </Box>
        <Group justify="space-between">
          {post.tags && (
            <Group className={classes.tags} justify="left">
              {post.tags.map((tag, idx) => (
                <Text key={idx} className={classes.tag} size="sm" fw="bold" span>
                  {`#${tag}`}
                </Text>
              ))}
            </Group>
          )}
          {RequireRole(Role.Admin, role) && (
            <Group justify="right">
              {onTogglePinned && (
                <ActionIcon disabled={disabled} onClick={() => onTogglePinned(post, setDisabled)}>
                  {post.isPinned ? <Icon path={mdiPinOffOutline} size={1} /> : <Icon path={mdiPinOutline} size={1} />}
                </ActionIcon>
              )}
              <ActionIcon component={Link} to={`/posts/${post.id}/edit`}>
                <Icon path={mdiPencilOutline} size={1} />
              </ActionIcon>
            </Group>
          )}
        </Group>
        <Group className={classes.footer} gap={5} justify="left" wrap="nowrap">
          <Avatar alt="avatar" src={post.authorAvatar} size="sm">
            {post.authorName?.slice(0, 1) ?? 'A'}
          </Avatar>
          <Text className={classes.metadata} fw={500} size="sm">
            {t('post.content.metadata', {
              author: post.authorName ?? 'Anonym',
              date: dayjs(post.time).format('lll'),
            })}
          </Text>
        </Group>
      </Stack>
    </Card>
  )
}
