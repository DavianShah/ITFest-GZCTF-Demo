import {
  Button,
  Card,
  SegmentedControl,
  SimpleGrid,
  Stack,
  Switch,
  Text,
  Textarea,
  TextInput,
  Title,
} from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { mdiCheck, mdiContentSaveOutline, mdiFlaskOutline } from '@mdi/js'
import { Icon } from '@mdi/react'
import { FC, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { WithGameEditTab } from '@Components/admin/WithGameEditTab'
import { showErrorMsg } from '@Utils/Shared'
import api, { BloodNotificationModel } from '@Api'
import adminClasses from '@Styles/Admin.module.css'

type EmbedTemplate = Pick<
  BloodNotificationModel,
  'embedTitleTemplate' | 'embedDescriptionTemplate' | 'embedColor' | 'embedFieldsTemplate' | 'embedFooterTemplate'
>

const DEFAULT_FIELDS = `👤 User / Team|{team}|true
🏁 Challenge|{challenge}|true
📂 Category|{category}|true
💯 Points|{score}|true
🎮 Game|{game}|true
🏆 Rank|#{rank}|true`

const TEMPLATES: Record<string, EmbedTemplate> = {
  Professional: {
    embedTitleTemplate: '{emoji} {blood} BLOOD!',
    embedDescriptionTemplate: '**{team}** conquered **{challenge}** and claimed rank **#{rank}**!',
    embedColor: '',
    embedFieldsTemplate: DEFAULT_FIELDS,
    embedFooterTemplate: 'Solved at {time} • ITFest CTF',
  },
  Dramatic: {
    embedTitleTemplate: '🚨 {emoji} {blood} BLOOD! 🚨',
    embedDescriptionTemplate: '**{team}** has conquered **{challenge}**. The battlefield has changed!',
    embedColor: '#ff0044',
    embedFieldsTemplate: DEFAULT_FIELDS,
    embedFooterTemplate: 'Claimed at {time} • ITFest CTF',
  },
  Minimal: {
    embedTitleTemplate: '{emoji} Blood #{rank}',
    embedDescriptionTemplate: '**{team}** solved **{challenge}**.',
    embedColor: '',
    embedFieldsTemplate: `Team|{team}|true
Challenge|{challenge}|true
Points|{score}|true`,
    embedFooterTemplate: '{time}',
  },
  'CTF-style': {
    embedTitleTemplate: '{emoji} [{category}] {blood} BLOOD',
    embedDescriptionTemplate: '`{team}` captured `{challenge}` for `{score}` points.',
    embedColor: '#00aaff',
    embedFieldsTemplate: `🏴 Team|{team}|true
🚩 Challenge|{challenge}|true
🎯 Rank|#{rank}|true
🔗 IDs|game:{gameId} / challenge:{challengeId} / submission:{submissionId}|false`,
    embedFooterTemplate: 'ITFest CTF • {time}',
  },
}

const BloodNotificationEdit: FC = () => {
  const { id } = useParams()
  const gameId = parseInt(id ?? '-1')
  const [settings, setSettings] = useState<BloodNotificationModel>()
  const [disabled, setDisabled] = useState(false)
  const { t } = useTranslation()

  useEffect(() => {
    if (gameId < 0) return

    api.edit
      .editGetGameBloodNotification(gameId)
      .then((response) => setSettings(response.data))
      .catch((error) => showErrorMsg(error, t))
  }, [gameId, t])

  const save = async () => {
    if (!settings) return
    setDisabled(true)
    try {
      const response = await api.edit.editUpdateGameBloodNotification(gameId, settings)
      setSettings(response.data)
      showNotification({
        color: 'teal',
        message: 'Blood notification settings saved.',
        icon: <Icon path={mdiCheck} size={1} />,
      })
    } catch (error) {
      showErrorMsg(error, t)
    } finally {
      setDisabled(false)
    }
  }

  const test = async () => {
    if (!settings) return
    setDisabled(true)
    try {
      await api.edit.editTestGameBloodNotification(gameId, settings)
      showNotification({
        color: 'teal',
        message: 'Test webhook sent.',
        icon: <Icon path={mdiCheck} size={1} />,
      })
    } catch (error) {
      showErrorMsg(error, t)
    } finally {
      setDisabled(false)
    }
  }

  return (
    <WithGameEditTab
      isLoading={!settings}
      contentPos="right"
      head={
        <>
          <Button
            variant="outline"
            disabled={disabled || !settings}
            leftSection={<Icon path={mdiFlaskOutline} size={1} />}
            onClick={test}
          >
            Test Webhook
          </Button>
          <Button
            disabled={disabled || !settings}
            leftSection={<Icon path={mdiContentSaveOutline} size={1} />}
            onClick={save}
          >
            Save
          </Button>
        </>
      }
    >
      {settings && (
        <Stack gap="lg" className={adminClasses.controlStack}>
          <Card withBorder className={adminClasses.controlCard}>
            <Stack>
              <Title order={3} className={adminClasses.controlHeader}>
                Discord Blood Alerts
              </Title>
              <Switch
                label="Enable blood notification"
                checked={settings.enabled}
                disabled={disabled}
                onChange={(event) => setSettings({ ...settings, enabled: event.currentTarget.checked })}
              />
              <TextInput
                required={settings.enabled}
                label="Discord Webhook URL"
                placeholder="https://discord.com/api/webhooks/..."
                value={settings.discordWebhookUrl ?? ''}
                disabled={disabled}
                onChange={(event) => setSettings({ ...settings, discordWebhookUrl: event.currentTarget.value })}
              />
              <TextInput
                required
                label="Time zone"
                description="IANA time zone used for {time}. Asia/Jakarta renders WIB."
                placeholder="Asia/Jakarta"
                value={settings.timeZone}
                disabled={disabled}
                onChange={(event) => setSettings({ ...settings, timeZone: event.currentTarget.value })}
              />
              <Stack gap="xs">
                <Text size="sm" fw={500}>
                  Notify mode
                </Text>
                <SegmentedControl
                  fullWidth
                  disabled={disabled}
                  value={String(settings.maxRank ?? 1)}
                  data={[
                    { label: 'Firstblood only', value: '1' },
                    { label: 'Top 3 blood', value: '3' },
                    { label: 'Top 5 blood', value: '5' },
                    { label: 'All first-solves', value: '0' },
                  ]}
                  onChange={(value) => setSettings({ ...settings, maxRank: Number(value) })}
                />
              </Stack>
            </Stack>
          </Card>

          <Card withBorder className={adminClasses.controlCard}>
            <Stack>
              <Title order={4} className={adminClasses.controlHeader}>
                Embed customization
              </Title>
              <TextInput
                required={settings.enabled}
                label="Embed title template"
                value={settings.embedTitleTemplate}
                disabled={disabled}
                onChange={(event) => setSettings({ ...settings, embedTitleTemplate: event.currentTarget.value })}
              />
              <Textarea
                required={settings.enabled}
                autosize
                minRows={3}
                maxRows={8}
                label="Embed description template"
                value={settings.embedDescriptionTemplate || settings.template}
                disabled={disabled}
                onChange={(event) => setSettings({ ...settings, embedDescriptionTemplate: event.currentTarget.value })}
              />
              <TextInput
                label="Embed color"
                description="Optional six-digit hex color. Leave empty for automatic rank colors."
                placeholder="#ff0044"
                value={settings.embedColor ?? ''}
                disabled={disabled}
                onChange={(event) => setSettings({ ...settings, embedColor: event.currentTarget.value })}
              />
              <Textarea
                autosize
                minRows={7}
                maxRows={25}
                label="Embed fields template"
                description="One field per line using name|value|inline. Inline defaults to true."
                value={settings.embedFieldsTemplate}
                disabled={disabled}
                onChange={(event) => setSettings({ ...settings, embedFieldsTemplate: event.currentTarget.value })}
              />
              <TextInput
                required={settings.enabled}
                label="Embed footer template"
                value={settings.embedFooterTemplate}
                disabled={disabled}
                onChange={(event) => setSettings({ ...settings, embedFooterTemplate: event.currentTarget.value })}
              />
              <Text size="sm" c="dimmed">
                Available placeholders: {'{emoji}'}, {'{blood}'}, {'{rank}'}, {'{team}'}, {'{challenge}'},{' '}
                {'{category}'}, {'{score}'}, {'{game}'}, {'{time}'}, {'{submissionId}'}, {'{challengeId}'}, {'{gameId}'}
              </Text>
            </Stack>
          </Card>

          <Card withBorder className={adminClasses.controlCard}>
            <Stack>
              <Title order={4} className={adminClasses.controlHeader}>
                Suggested templates
              </Title>
              <SimpleGrid cols={3}>
                {Object.entries(TEMPLATES).map(([name, template]) => (
                  <Button
                    key={name}
                    variant="light"
                    disabled={disabled}
                    onClick={() => setSettings({ ...settings, ...template })}
                  >
                    {name}
                  </Button>
                ))}
              </SimpleGrid>
            </Stack>
          </Card>
        </Stack>
      )}
    </WithGameEditTab>
  )
}

export default BloodNotificationEdit
