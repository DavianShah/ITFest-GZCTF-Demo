import { Group, GroupProps, Stack, Text, Title } from '@mantine/core'
import { forwardRef } from 'react'
import { LogoBox } from '@Components/LogoBox'
import classes from '@Styles/LogoHeader.module.css'

export const LogoHeader = forwardRef<HTMLDivElement, GroupProps>((props, ref) => {
  return (
    <Group
      ref={ref}
      wrap="nowrap"
      align="center"
      justify="flex-start"
      gap="sm"
      {...props}
      className={`${classes.root} ${props.className ?? ''}`}
    >
      <LogoBox size="42px" className={classes.mark} />
      <Stack gap={0} className={classes.wordmark}>
        <Title textWrap="nowrap" className={classes.title}>
          HACKTODAY
        </Title>
        <Text className={classes.kicker}>IT TODAY 2026</Text>
      </Stack>
    </Group>
  )
})
