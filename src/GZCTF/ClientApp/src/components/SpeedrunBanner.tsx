import { Alert, Badge, Group, Stack, Text } from '@mantine/core'
import { FC, useCallback, useEffect, useRef, useState } from 'react'
import { useParams } from 'react-router'
import api, { SpeedrunRoundStatus, SpeedrunStateModel } from '@Api'
import competition from '@Styles/Competition.module.css'

export const SpeedrunBanner: FC = () => {
  const { id } = useParams()
  const [state, setState] = useState<SpeedrunStateModel>()
  const lastFingerprint = useRef('')
  const gameId = Number(id)

  const refresh = useCallback(async () => {
    if (!id) return
    const next = (await api.game.gameGetSpeedrunState(gameId)).data
    const round = next.currentRound
    const fingerprint = `${round?.id ?? 'none'}:${round?.status ?? 'none'}:${round?.category ?? 'none'}`
    const visibilityChanged = fingerprint !== lastFingerprint.current
    lastFingerprint.current = fingerprint
    setState(next)

    if (visibilityChanged || round?.status === SpeedrunRoundStatus.Overtime)
      await api.game.mutateGameChallengesWithTeamInfo(gameId)
  }, [gameId, id])

  useEffect(() => {
    refresh()
    const pollTimer = window.setInterval(refresh, 3000)
    return () => window.clearInterval(pollTimer)
  }, [refresh])

  const round = state?.currentRound
  const overtime = round?.status === SpeedrunRoundStatus.Overtime

  if (!state?.isSpeedrun) return null

  return (
    <Alert
      color={overtime ? 'red' : 'blue'}
      title={overtime ? 'Speedrun overtime' : 'Speedrun mode'}
      className={competition.speedrunBanner}
    >
      <Stack gap={4}>
        <Group>
          <Text fw={700}>
            {round?.category ? `Current Speedrun category: ${round.category}` : 'Waiting for next Speedrun spin...'}
          </Text>
          {round && <Badge color={overtime ? 'red' : 'blue'}>{round.status}</Badge>}
        </Group>
        <Text size="sm">Scores still count toward the main scoreboard.</Text>
        {round?.status === SpeedrunRoundStatus.Ready && (
          <Text>Category selected. Waiting for the admin to start the round.</Text>
        )}
        {overtime && <Text>{state.message}</Text>}
      </Stack>
    </Alert>
  )
}
