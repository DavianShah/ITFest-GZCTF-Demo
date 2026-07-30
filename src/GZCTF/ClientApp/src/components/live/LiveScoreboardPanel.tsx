import { FC } from 'react'
import { LiveScoreboardTeamModel } from '@Api'
import classes from '@Styles/LiveScoreboard.module.css'

const podiumClass = (rank?: number) => {
  if (rank === 1) return classes.podium1
  if (rank === 2) return classes.podium2
  if (rank === 3) return classes.podium3
  return ''
}

export const LiveScoreboardPanel: FC<{ teams: LiveScoreboardTeamModel[]; changedTeams: Set<number> }> = ({
  teams, changedTeams,
}) => <aside className={`${classes.hudPanel} ${classes.scoreboardPanel}`}>
  <div className={classes.panelHead}><div><b>SCOREBOARD</b><span>TOP 10</span></div><i>RANK / SCORE / SOLVES</i></div>
  <div className={classes.scoreRows}>{teams.map((team) => <div key={team.id}
    className={`${classes.scoreRow} ${podiumClass(team.rank)} ${changedTeams.has(team.id!) ? classes.scorePulse : ''}`}>
    <strong>{String(team.rank ?? 0).padStart(2, '0')}</strong><b>{team.name}</b><span>{team.score?.toLocaleString()}</span><em>{team.solvedCount} ✓</em>
  </div>)}</div>
</aside>
