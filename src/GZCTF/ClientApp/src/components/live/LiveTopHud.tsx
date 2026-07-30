import { FC } from 'react'
import { SpeedrunRoundModel, SpeedrunRoundStatus } from '@Api'
import { formatDurationSeconds } from '@Utils/Shared'
import classes from '@Styles/LiveScoreboard.module.css'

export const LiveTopHud: FC<{
  title: string
  subtitle?: string | null
  round?: SpeedrunRoundModel | null
  remainingSeconds: number
  concealCategory?: boolean
}> = ({ title, subtitle, round, remainingSeconds, concealCategory }) => {
  const active = round?.status === SpeedrunRoundStatus.Running || round?.status === SpeedrunRoundStatus.Overtime
  const urgent = active && remainingSeconds <= 30
  const critical = active && remainingSeconds <= 10
  const timer = round?.status === SpeedrunRoundStatus.Ready ? 'READY' : active ? formatDurationSeconds(remainingSeconds) : '--:--'

  return <header className={classes.topHud}>
    <div className={classes.brand}><strong>{title}</strong>{subtitle && <span>{subtitle}</span>}</div>
    <div className={`${classes.hudTimer} ${urgent ? classes.urgent : ''} ${critical ? classes.critical : ''}`}>{timer}</div>
    <div className={classes.liveStatus}><i /> <b>LIVE</b><span>|</span><strong>{concealCategory ? 'SCANNING' : round?.category ?? 'STANDBY'}</strong><span>|</span><em>{concealCategory ? 'SPINNING' : round?.status ?? 'WAITING'}</em></div>
  </header>
}
