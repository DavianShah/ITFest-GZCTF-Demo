import { FC } from 'react'
import { LiveAnnouncement } from '@Components/live/types'
import classes from '@Styles/LiveScoreboard.module.css'

const announcementClasses: Partial<Record<LiveAnnouncement['kind'], string>> = {
  firstBlood: classes.announcement_firstBlood,
  blood: classes.announcement_blood,
  hint: classes.announcement_hint,
  overtime: classes.announcement_overtime,
  countdown: classes.announcement_countdown,
}

export const LiveAnnouncementOverlay: FC<{ event?: LiveAnnouncement }> = ({ event }) => event ? (
  <div className={`${classes.announcement} ${announcementClasses[event.kind] ?? ''}`}>
    <div className={classes.shockwave} /><div className={classes.shockwaveTwo} />
    <strong>{event.title}</strong>{event.text && <span>{event.text}</span>}
  </div>
) : null
