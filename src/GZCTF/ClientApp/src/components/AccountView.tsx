import { Box, Center, Paper, Stack, Title } from '@mantine/core'
import { FC, PropsWithChildren, ReactNode } from 'react'
import { useNavigate } from 'react-router'
import { LogoHeader } from '@Components/LogoHeader'
import classes from '@Styles/AccountView.module.css'

interface AccountViewProps extends PropsWithChildren {
  onSubmit?: (event: React.SubmitEvent<HTMLFormElement>) => Promise<void>
  title?: ReactNode
}

export const AccountView: FC<AccountViewProps> = ({ onSubmit, title, children }) => {
  const navigate = useNavigate()

  return (
    <Center className={classes.viewport}>
      <Box className={classes.shell}>
        <LogoHeader className={classes.brand} onClick={() => navigate('/')} />
        <Paper className={classes.panel} radius={0}>
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
