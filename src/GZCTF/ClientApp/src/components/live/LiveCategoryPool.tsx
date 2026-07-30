import { FC } from 'react'
import { ChallengeCategory, SpeedrunRoundModel, SpeedrunRoundStatus } from '@Api'
import classes from '@Styles/LiveScoreboard.module.css'

const Pills: FC<{ values: ChallengeCategory[]; className: string }> = ({ values, className }) => <>
  {values.length ? values.map(value => <span className={`${classes.categoryPill} ${className}`} key={value}>{value}</span>) : <span className={classes.emptyPill}>NONE</span>}
</>

export const LiveCategoryPool: FC<{
  round?: SpeedrunRoundModel | null
  available: ChallengeCategory[]
  used: ChallengeCategory[]
  concealActive?: boolean
}> = ({
  round, available, used, concealActive,
}) => <footer className={`${classes.hudPanel} ${classes.categoryPool}`}>
  <div><b>ACTIVE</b><Pills values={!concealActive && round?.category ? [round.category] : []}
    className={round?.status === SpeedrunRoundStatus.Ready ? classes.selectedPill : classes.activePill} /></div>
  <div><b>AVAILABLE</b><Pills values={available} className={classes.availablePill} /></div>
  <div><b>USED</b><Pills values={used} className={classes.usedPill} /></div>
</footer>
