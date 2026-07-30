import { Button, Card, Group, Select, SimpleGrid, Slider, Stack, Switch, TextInput, Title } from '@mantine/core'
import { notifications } from '@mantine/notifications'
import { FC, useEffect, useState } from 'react'
import { useParams } from 'react-router'
import api, { LiveScoreboardConfigModel, LiveScoreboardSoundModel, LiveScoreboardVisualIntensity } from '@Api'
import { WithGameEditTab } from '@Components/admin/WithGameEditTab'
import { handleAxiosError } from '@Utils/ApiHelper'

const soundNames: (keyof LiveScoreboardSoundModel)[] = [
  'spin', 'categorySelected', 'gameStart', 'hintDrop', 'firstBlood', 'secondBlood', 'thirdBlood',
  'correctSubmit', 'wrongSubmit', 'reminder', 'countdownTick', 'overtime', 'roundFinished', 'scoreUpdate',
]

const LiveScoreboardEdit: FC = () => {
  const { id } = useParams()
  const gameId = Number(id)
  const [config, setConfig] = useState<LiveScoreboardConfigModel>()
  const [busy, setBusy] = useState(false)

  const load = async () => setConfig((await api.edit.editGetLiveScoreboardConfig(gameId)).data)
  useEffect(() => { void load() }, [gameId])

  const run = async (action: () => Promise<unknown>) => {
    setBusy(true)
    try {
      await action()
      await load()
      notifications.show({ color: 'teal', message: 'Live Scoreboard settings saved.' })
    } catch (error) {
      notifications.show({ color: 'red', message: await handleAxiosError(error) })
    } finally {
      setBusy(false)
    }
  }

  return (
    <WithGameEditTab isLoading={!config}>
      {config && <Stack>
        <Card withBorder><Stack>
          <Title order={3}>Live Scoreboard</Title>
          <Switch label="Enable Live Scoreboard" checked={config.enabled ?? false}
            onChange={(event) => setConfig({ ...config, enabled: event.currentTarget.checked })} />
          <TextInput label="Display title" value={config.title}
            onChange={(event) => setConfig({ ...config, title: event.currentTarget.value })} />
          <TextInput label="Subtitle / tagline" value={config.subtitle ?? ''}
            onChange={(event) => setConfig({ ...config, subtitle: event.currentTarget.value })} />
          <Select label="Visual intensity" value={config.visualIntensity ?? LiveScoreboardVisualIntensity.Normal}
            data={Object.values(LiveScoreboardVisualIntensity)}
            onChange={(value) => setConfig({ ...config, visualIntensity: value as LiveScoreboardVisualIntensity })} />
          <Group>
            <Button disabled={busy} onClick={() => run(() => api.edit.editUpdateLiveScoreboardConfig(gameId, config))}>Save settings</Button>
            <Button component="a" href={`/games/${gameId}/live`} target="_blank" variant="light">Open Live Scoreboard</Button>
          </Group>
        </Stack></Card>
        <Card withBorder><Stack>
          <Title order={3}>Stage audio</Title>
          <Switch label="Enable sound effects" checked={config.soundEnabled ?? true}
            onChange={(event) => setConfig({ ...config, soundEnabled: event.currentTarget.checked })} />
          <Slider label={(value) => `${value}%`} value={(config.volume ?? 0.7) * 100}
            onChange={(value) => setConfig({ ...config, volume: value / 100 })} />
          <SimpleGrid cols={{ base: 1, md: 2 }}>
            {soundNames.map((name) => <TextInput key={name} label={`${name} sound URL`}
              placeholder="Empty uses fallback tone" value={config.sounds?.[name] ?? ''}
              onChange={(event) => setConfig({
                ...config, sounds: { ...config.sounds, [name]: event.currentTarget.value || null },
              })} />)}
          </SimpleGrid>
          <Group>
            <Button disabled={busy} onClick={() => run(() => api.edit.editUpdateLiveScoreboardConfig(gameId, config))}>Save audio</Button>
            <Button variant="outline" disabled={busy} onClick={() => run(() => api.edit.editResetLiveScoreboardSounds(gameId))}>Reset sounds</Button>
          </Group>
        </Stack></Card>
      </Stack>}
    </WithGameEditTab>
  )
}

export default LiveScoreboardEdit
