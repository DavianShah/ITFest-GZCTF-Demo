import { Box, Center, Paper, Stack, Text, Title } from '@mantine/core'
import { FC, PropsWithChildren, ReactNode } from 'react'
import { useNavigate } from 'react-router'
import { LogoHeader } from '@Components/LogoHeader'
import { useConfig } from '@Hooks/useConfig'
import classes from '@Styles/AccountView.module.css'

interface AccountViewProps extends PropsWithChildren {
  onSubmit?: (event: React.SubmitEvent<HTMLFormElement>) => Promise<void>
  title?: ReactNode
}

export const AccountView: FC<AccountViewProps> = ({ onSubmit, title, children }) => {
  const navigate = useNavigate()
  const { config } = useConfig()

  return (
    <Center className={classes.viewport}>
      <Box className={classes.shell}>
        <Box className={classes.identity}>
          <LogoHeader className={classes.brand} onClick={() => navigate('/')} />
          <Text className={classes.presenter}>HIMALKOM IPB PRESENTS</Text>
          <Title order={1} className={classes.manifesto}>
            <Text component="span">SHRED</Text>
            <Text component="span">THE CODE.</Text>
            <Text component="span">OWN THE</Text>
            <Text component="span">SYSTEM.</Text>
          </Title>
          <Text className={classes.slogan}>{config?.slogan ?? 'Hack for fun not for profit'}</Text>
        </Box>
        <Paper className={classes.panel} radius={0}>
          <Text className={classes.terminalLabel}>TERMINAL_ACCESS</Text>
          {title && (
            <Title order={1} className={classes.title}>
              {title}
            </Title>
          )}
          <form className={classes.form} onSubmit={onSubmit}>
            <Stack className={classes.content} align="center" justify="center">
              {children}
            </Stack>
          </form>
        </Paper>
      </Box>
    </Center>
  )
}
