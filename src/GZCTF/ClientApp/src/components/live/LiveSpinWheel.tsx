import { FC } from 'react'
import { ChallengeCategory } from '@Api'
import classes from '@Styles/LiveScoreboard.module.css'

export const LiveSpinWheel: FC<{
  categories: ChallengeCategory[]
  phase: 'idle' | 'spinning' | 'revealed'
  selected?: ChallengeCategory
}> = ({
  categories, phase, selected,
}) => {
  const labels = categories.length ? categories : selected ? [selected] : []
  return <div className={`${classes.wheelShell} ${phase === 'spinning' ? classes.spinActive : ''}
    ${phase === 'revealed' ? classes.spinRevealed : ''}`}>
    <div className={classes.wheelPointer} />
    <div className={classes.wheel}>
      {labels.map((category, index) => {
        const angle = (360 / labels.length) * index
        return <span key={category} className={classes.wheelCategory}
          style={{ transform: `rotate(${angle}deg) translateY(-10.5rem) rotate(${-angle}deg)` }}>{category}</span>
      })}
      <div className={classes.wheelCore}><span>{phase === 'spinning' ? 'SPINNING' : selected ?? 'SPIN'}</span></div>
    </div>
  </div>
}
