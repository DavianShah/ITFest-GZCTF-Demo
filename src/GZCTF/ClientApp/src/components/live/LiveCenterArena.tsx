import { FC } from 'react'
import { ChallengeCategory, LiveScoreboardTeamModel, SpeedrunRoundModel, SpeedrunRoundStatus } from '@Api'
import { LiveSpinWheel } from '@Components/live/LiveSpinWheel'
import { LiveTeamOrbit } from '@Components/live/LiveTeamOrbit'
import classes from '@Styles/LiveScoreboard.module.css'

export const LiveCenterArena: FC<{
  round?: SpeedrunRoundModel | null
  remaining: ChallengeCategory[]
  spinPhase: 'idle' | 'spinning' | 'revealed'
  teams: LiveScoreboardTeamModel[]
  attackingTeams: Set<number>
  bloodTeams: Set<number>
}> = ({ round, remaining, spinPhase, teams, attackingTeams, bloodTeams }) => {
  let eyebrow = 'SYSTEM STANDBY'
  let title = <>WAITING<br />FOR NEXT SPIN</>
  let detail = remaining.length ? `Remaining categories: ${remaining.join(' • ')}` : 'All configured categories have been used'

  if (spinPhase === 'spinning') {
    eyebrow = 'ROLE SELECTION'; title = <>SPINNING...</>; detail = 'Target acquisition in progress'
  } else if (round?.status === SpeedrunRoundStatus.Ready) {
    eyebrow = 'CATEGORY LOCKED'; title = <>{String(round.category)}</>; detail = 'Waiting for admin to start...'
  } else if (round?.status === SpeedrunRoundStatus.Running) {
    eyebrow = 'TARGET LOCKED'; title = <>{String(round.category)}</>; detail = 'Scores count toward the main scoreboard'
  } else if (round?.status === SpeedrunRoundStatus.Overtime) {
    eyebrow = 'OVERCLOCK TARGET'; title = <>{String(round.category)}</>; detail = 'UNSOLVED CHALLENGES REMAIN'
  }

  return <section className={`${classes.centerArena} ${round?.status === SpeedrunRoundStatus.Overtime ? classes.overclock : ''}
    ${spinPhase === 'spinning' ? classes.arenaSpinning : ''}`}>
    <div className={classes.radarRingOne} /><div className={classes.radarRingTwo} /><div className={classes.radarSweep} />
    <LiveTeamOrbit teams={teams} attackingTeams={attackingTeams} bloodTeams={bloodTeams} />
    <LiveSpinWheel categories={remaining} phase={spinPhase} selected={round?.category} />
    <div className={classes.centerCore}>
      <div className={classes.coreHexagon} />
      <div className={classes.centerCopy}><span>{eyebrow}</span><h1>{title}</h1><p>{detail}</p></div>
    </div>
  </section>
}
