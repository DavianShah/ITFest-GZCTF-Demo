import { FC } from 'react'
import classes from '@Styles/LiveScoreboard.module.css'

export const LiveOceanBackground: FC = () => (
  <div className={classes.ocean} aria-hidden>
    <div className={classes.waveOne} />
    <div className={classes.waveTwo} />
    {Array.from({ length: 16 }, (_, index) => <i key={index} className={classes.bubble} style={{
      left: `${4 + (index * 17) % 94}%`,
      animationDelay: `${-(index * 1.7)}s`,
      animationDuration: `${9 + index % 8}s`,
      width: `${6 + index % 5 * 4}px`,
      height: `${6 + index % 5 * 4}px`,
    }} />)}
    <span className={`${classes.fish} ${classes.fishOne}`}>◀</span>
    <span className={`${classes.fish} ${classes.fishTwo}`}>◀</span>
    <span className={`${classes.fish} ${classes.fishThree}`}>▶</span>
    <div className={`${classes.jellyfish} ${classes.jellyOne}`} />
    <div className={`${classes.jellyfish} ${classes.jellyTwo}`} />
  </div>
)
