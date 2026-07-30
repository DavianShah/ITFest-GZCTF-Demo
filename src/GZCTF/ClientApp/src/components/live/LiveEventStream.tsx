import { FC } from 'react'
import { LiveScoreboardEventModel, NoticeType } from '@Api'
import classes from '@Styles/LiveScoreboard.module.css'

const eventMeta = (event: LiveScoreboardEventModel) => {
  if (event.type === NoticeType.FirstBlood) return ['BLOOD', classes.bloodEvent]
  if (event.type === NoticeType.SecondBlood || event.type === NoticeType.ThirdBlood) return ['BLOOD', classes.bloodEvent]
  if (event.message?.startsWith('Hint #')) return ['HINT', classes.hintEvent]
  if (event.message?.includes('category selected')) return ['SPIN', classes.roundEvent]
  if (event.message?.includes('round')) return ['ROUND', classes.roundEvent]
  if (event.message?.includes('Overtime')) return ['OVERTIME', classes.bloodEvent]
  return ['SYSTEM', classes.normalEvent]
}

export const LiveEventStream: FC<{ events: LiveScoreboardEventModel[] }> = ({ events }) => <aside className={`${classes.hudPanel} ${classes.eventPanel}`}>
  <div className={classes.panelHead}><div><b>EVENT STREAM</b><span>LIVE TELEMETRY</span></div><i className={classes.streamDot} /></div>
  <div className={classes.eventRows}>{events.slice(0, 10).map((event) => {
    const [label, color] = eventMeta(event)
    return <div className={`${classes.eventRow} ${color}`} key={event.id}><span>[{label}]</span><p>{event.message}</p></div>
  })}</div>
</aside>
