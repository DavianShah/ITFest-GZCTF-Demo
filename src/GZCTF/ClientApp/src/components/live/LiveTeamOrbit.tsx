import { CSSProperties, FC } from 'react'
import { LiveScoreboardTeamModel } from '@Api'
import classes from '@Styles/LiveScoreboard.module.css'

const shortName = (name?: string) => {
  const value = name?.trim() || 'TEAM'
  return value.length > 14 ? `${value.slice(0, 13)}…` : value
}

export const LiveTeamOrbit: FC<{
  teams: LiveScoreboardTeamModel[]
  attackingTeams: Set<number>
  bloodTeams: Set<number>
}> = ({ teams, attackingTeams, bloodTeams }) => {
  const visible = teams.slice(0, 10)
  return <div className={classes.teamOrbit} aria-hidden>
    <div className={classes.teamOrbitRing} />
    <div className={classes.teamOrbitTrack}>
      {visible.map((team, index) => {
        const angle = visible.length ? (index / visible.length) * 360 : 0
        const attacking = attackingTeams.has(team.id!)
        const blood = bloodTeams.has(team.id!)
        const style = { '--team-angle': `${angle}deg`, '--team-counter-angle': `${-angle}deg` } as CSSProperties
        return <div className={`${classes.teamOrbitPosition} ${attacking ? classes.teamAttacking : ''}
          ${blood ? classes.teamBloodAttack : ''}`} style={style} key={team.id}>
          {attacking && <div className={classes.teamBeam} />}
          <div className={`${classes.teamNode} ${team.rank === 1 ? classes.teamNodeTop1 : ''}
            ${team.rank === 2 || team.rank === 3 ? classes.teamNodeTopThree : ''}`}>
            <strong>#{team.rank}</strong>
            <span>{shortName(team.name)}</span>
            <em>{team.score?.toLocaleString()}</em>
          </div>
        </div>
      })}
    </div>
    {attackingTeams.size > 0 && <div className={`${classes.targetImpact} ${bloodTeams.size ? classes.bloodImpact : ''}`} />}
  </div>
}
