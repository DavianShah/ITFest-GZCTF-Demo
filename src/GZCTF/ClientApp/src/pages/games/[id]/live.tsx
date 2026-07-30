import { FC, useCallback, useEffect, useRef, useState } from 'react'
import { useParams } from 'react-router'
import { GameMode, NoticeType, SpeedrunRoundStatus } from '@Api'
import { LiveAnnouncementOverlay } from '@Components/live/LiveAnnouncementOverlay'
import { LiveCategoryPool } from '@Components/live/LiveCategoryPool'
import { LiveCenterArena } from '@Components/live/LiveCenterArena'
import { LiveEventStream } from '@Components/live/LiveEventStream'
import { LiveOceanBackground } from '@Components/live/LiveOceanBackground'
import { LiveScoreboardPanel } from '@Components/live/LiveScoreboardPanel'
import { LiveTopHud } from '@Components/live/LiveTopHud'
import { LiveAnnouncement } from '@Components/live/types'
import { useLiveEventQueue } from '@Hooks/useLiveEventQueue'
import { useLiveState } from '@Hooks/useLiveState'
import { useStageSound } from '@Hooks/useStageSound'
import { formatDurationSeconds } from '@Utils/Shared'
import classes from '@Styles/LiveScoreboard.module.css'

const reminderPoints = [1200, 600, 300, 180, 60, 30, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1]
const SPIN_DURATION_MS = 6500
const REVEAL_DURATION_MS = 2500

const LiveScoreboardPage: FC = () => {
  const gameId = Number(useParams().id)
  const { state, round, active, remainingSeconds } = useLiveState(gameId)
  const { audioEnabled, unlockAudio, play } = useStageSound(state?.config)
  const { active: announcement, enqueue, markSeen } = useLiveEventQueue(useCallback((event: LiveAnnouncement) => play(event.sound), [play]))
  const initialized = useRef(false)
  const previousRound = useRef<string | undefined>(undefined)
  const previousScores = useRef(new Map<number, number>())
  const reminderKeys = useRef(new Set<string>())
  const spinTimers = useRef<number[]>([])
  const attackTimer = useRef<number | undefined>(undefined)
  const [spinPhase, setSpinPhase] = useState<'idle' | 'spinning' | 'revealed'>('idle')
  const [changedTeams, setChangedTeams] = useState(new Set<number>())
  const [bloodAttackTeams, setBloodAttackTeams] = useState(new Set<number>())

  useEffect(() => {
    if (!state) return
    const eventIds = (state.recentEvents ?? []).flatMap(event => event.id ? [event.id] : [])
    const fingerprint = `${round?.id ?? 'none'}:${round?.status ?? 'none'}`
    if (!initialized.current) {
      markSeen(eventIds)
      state.topTeams?.forEach(team => previousScores.current.set(team.id!, team.score ?? 0))
      previousRound.current = fingerprint
      setSpinPhase(round?.status === SpeedrunRoundStatus.Ready ? 'revealed' : 'idle')
      initialized.current = true
      return
    }

    if (fingerprint !== previousRound.current) {
      spinTimers.current.forEach(timer => window.clearTimeout(timer))
      spinTimers.current = []
      if (round?.status === SpeedrunRoundStatus.Ready) {
        setSpinPhase('spinning')
        play('spin')
        for (const delay of [3600, 4400, 5100, 5650, 6100])
          spinTimers.current.push(window.setTimeout(() => play('countdownTick'), delay))
        spinTimers.current.push(window.setTimeout(() => {
          setSpinPhase('revealed')
          enqueue({ key: `category-${round.id}`, kind: 'category', title: 'CATEGORY LOCKED', text: round.category,
            sound: 'categorySelected', duration: REVEAL_DURATION_MS })
        }, SPIN_DURATION_MS))
      } else if (round?.status === SpeedrunRoundStatus.Running) {
        setSpinPhase('idle')
        enqueue({ key: `start-${round.id}`, kind: 'start', title: 'GAME START', text: `CATEGORY: ${round.category}`,
          sound: 'gameStart', duration: 3800 })
      } else if (round?.status === SpeedrunRoundStatus.Overtime) {
        setSpinPhase('idle')
        enqueue({ key: `overtime-${round.id}`, kind: 'overtime', title: 'OVERCLOCK TIME',
          text: 'UNSOLVED CHALLENGES REMAIN', sound: 'overtime', duration: 4400 })
      } else if (!round && previousRound.current && previousRound.current !== 'none:none') {
        setSpinPhase('idle')
        enqueue({ key: `finished-${previousRound.current}`, kind: 'finished', title: 'ROUND FINISHED',
          text: 'WAITING FOR NEXT SPIN', sound: 'roundFinished', duration: 3500 })
      }
      previousRound.current = fingerprint
    }

    const bloodTeams = new Set<string>()
    for (const event of state.recentEvents ?? []) {
      if (!event.id || eventIds.length === 0) continue
      const blood = event.type === NoticeType.FirstBlood || event.type === NoticeType.SecondBlood || event.type === NoticeType.ThirdBlood
      if (blood && event.teamName) bloodTeams.add(event.teamName)
      if (event.type === NoticeType.FirstBlood) enqueue({ key: event.id, kind: 'firstBlood', title: 'FIRST BLOOD!',
        text: `${event.teamName} solved ${event.challengeTitle}`, sound: 'firstBlood', duration: 4800 })
      else if (event.type === NoticeType.SecondBlood) enqueue({ key: event.id, kind: 'blood', title: 'SECOND BLOOD!',
        text: `${event.teamName} solved ${event.challengeTitle}`, sound: 'secondBlood', duration: 3600 })
      else if (event.type === NoticeType.ThirdBlood) enqueue({ key: event.id, kind: 'blood', title: 'THIRD BLOOD!',
        text: `${event.teamName} solved ${event.challengeTitle}`, sound: 'thirdBlood', duration: 3600 })
      else if (event.message?.startsWith('Hint #')) enqueue({ key: event.id, kind: 'hint', title: 'HINT DROPPED',
        text: event.message, sound: 'hintDrop', duration: 3400 })
      else markSeen([event.id])
    }

    const changed = new Set<number>()
    for (const team of state.topTeams ?? []) {
      const oldScore = previousScores.current.get(team.id!)
      if (oldScore !== undefined && (team.score ?? 0) > oldScore) {
        changed.add(team.id!)
        if (!bloodTeams.has(team.name ?? '')) enqueue({ key: `score-${team.id}-${team.score}`, kind: 'correct',
          title: 'CORRECT!', text: `${team.name} +${(team.score ?? 0) - oldScore} POINTS`, sound: 'correctSubmit', duration: 2400 })
      }
      previousScores.current.set(team.id!, team.score ?? 0)
    }
    if (changed.size) {
      setChangedTeams(changed)
      setBloodAttackTeams(new Set((state.topTeams ?? []).filter(team =>
        changed.has(team.id!) && bloodTeams.has(team.name ?? '')).map(team => team.id!)))
      window.clearTimeout(attackTimer.current)
      attackTimer.current = window.setTimeout(() => {
        setChangedTeams(new Set())
        setBloodAttackTeams(new Set())
      }, 2600)
    }
  }, [enqueue, markSeen, play, round, state])

  useEffect(() => {
    if (!active || !round?.id || !reminderPoints.includes(remainingSeconds)) return
    const key = `time-${round.id}-${round.status}-${remainingSeconds}`
    if (reminderKeys.current.has(key)) return
    reminderKeys.current.add(key)
    enqueue({
      key, kind: remainingSeconds <= 10 ? 'countdown' : 'reminder',
      title: remainingSeconds <= 10 ? String(remainingSeconds) : `${formatDurationSeconds(remainingSeconds)} LEFT`,
      sound: remainingSeconds <= 10 ? 'countdownTick' : 'reminder',
      duration: remainingSeconds <= 10 ? 800 : 2100,
    })
  }, [active, enqueue, remainingSeconds, round?.id, round?.status])

  useEffect(() => () => {
    spinTimers.current.forEach(timer => window.clearTimeout(timer))
    window.clearTimeout(attackTimer.current)
  }, [])

  if (!state) return <div className={classes.fallback}><b>CONNECTING TO LIVE ARENA</b><span>Synchronizing stage telemetry...</span></div>
  if (state.gameMode !== GameMode.Speedrun) return <div className={classes.fallback}><b>LIVE SCOREBOARD</b><span>Available for Speedrun mode.</span></div>
  if (!state.config?.enabled) return <div className={classes.fallback}><b>LIVE SCOREBOARD</b><span>Currently disabled.</span></div>

  const overtime = round?.status === SpeedrunRoundStatus.Overtime
  return <main className={`${classes.stage} ${overtime ? classes.stageOvertime : ''} ${state.config.visualIntensity === 'Hype' ? classes.stageHype : ''}`}>
    <LiveOceanBackground />
    <LiveTopHud title={state.config.title} subtitle={state.config.subtitle} round={round} remainingSeconds={remainingSeconds}
      concealCategory={spinPhase === 'spinning'} />
    <div className={classes.mainGrid}>
      <LiveEventStream events={state.recentEvents ?? []} />
      <LiveCenterArena round={round} remaining={state.speedrunState?.remainingCategories ?? []} spinPhase={spinPhase}
        teams={state.topTeams ?? []} attackingTeams={changedTeams} bloodTeams={bloodAttackTeams} />
      <LiveScoreboardPanel teams={state.topTeams ?? []} changedTeams={changedTeams} />
    </div>
    <LiveCategoryPool round={round} available={state.speedrunState?.remainingCategories ?? []}
      used={state.speedrunState?.usedCategories ?? []} concealActive={spinPhase === 'spinning'} />
    <LiveAnnouncementOverlay event={announcement} />
    <button className={`${classes.audioButton} ${audioEnabled ? classes.audioOn : ''}`} onClick={unlockAudio}>
      <i /> {audioEnabled ? 'AUDIO ON' : 'ENABLE STAGE AUDIO'}
    </button>
  </main>
}

export default LiveScoreboardPage
