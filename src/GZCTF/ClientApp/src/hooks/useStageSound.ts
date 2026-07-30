import { useCallback, useEffect, useRef, useState } from 'react'
import { LiveScoreboardConfigModel } from '@Api'
import { StageSoundName } from '@Components/live/types'

const patterns: Record<StageSoundName, [number, number, OscillatorType][]> = {
  spin: [[180, .22, 'sine'], [220, .22, 'sine'], [270, .22, 'sine'], [330, .22, 'sine'], [400, .22, 'sine'],
    [480, .22, 'sine'], [570, .22, 'sine'], [680, .22, 'sine'], [810, .25, 'sine'], [960, .32, 'sine']],
  categorySelected: [[523, .16, 'sine'], [659, .16, 'sine'], [784, .3, 'sine']],
  gameStart: [[262, .18, 'triangle'], [392, .18, 'triangle'], [523, .45, 'triangle']],
  hintDrop: [[880, .1, 'sine'], [1175, .12, 'sine'], [1568, .25, 'sine']],
  firstBlood: [[90, .35, 'sawtooth'], [180, .25, 'triangle'], [720, .45, 'sine']],
  secondBlood: [[130, .25, 'sawtooth'], [520, .3, 'sine']],
  thirdBlood: [[160, .22, 'triangle'], [440, .28, 'sine']],
  correctSubmit: [[523, .12, 'sine'], [659, .12, 'sine'], [784, .2, 'sine']],
  wrongSubmit: [[220, .16, 'square'], [155, .25, 'square']],
  reminder: [[500, .22, 'triangle']],
  countdownTick: [[760, .09, 'square']],
  overtime: [[180, .2, 'sawtooth'], [260, .2, 'sawtooth'], [180, .3, 'sawtooth']],
  roundFinished: [[523, .18, 'sine'], [392, .18, 'sine'], [262, .35, 'sine']],
  scoreUpdate: [[620, .08, 'sine'], [820, .12, 'sine']],
}

export const useStageSound = (config?: LiveScoreboardConfigModel) => {
  const [enabled, setEnabled] = useState(false)
  const context = useRef<AudioContext | undefined>(undefined)
  const configRef = useRef(config)

  useEffect(() => {
    configRef.current = config
  }, [config])

  const unlock = useCallback(() => {
    const ctx = context.current ?? new AudioContext()
    context.current = ctx
    void ctx.resume()
    setEnabled(true)
  }, [])

  const fallback = useCallback((name: StageSoundName) => {
    const ctx = context.current
    if (!ctx) return
    const volume = configRef.current?.volume ?? .75
    let at = ctx.currentTime
    for (const [frequency, duration, type] of patterns[name]) {
      const oscillator = ctx.createOscillator()
      const gain = ctx.createGain()
      oscillator.type = type
      oscillator.frequency.setValueAtTime(frequency, at)
      gain.gain.setValueAtTime(volume * .14, at)
      gain.gain.exponentialRampToValueAtTime(.001, at + duration)
      oscillator.connect(gain).connect(ctx.destination)
      oscillator.start(at)
      oscillator.stop(at + duration)
      at += duration * .75
    }
  }, [])

  const play = useCallback((name: StageSoundName) => {
    const currentConfig = configRef.current
    if (!enabled || !currentConfig?.soundEnabled) return
    const url = currentConfig.sounds?.[name]
    if (!url) return fallback(name)
    const audio = new Audio(url)
    audio.volume = currentConfig.volume ?? .75
    void audio.play().catch(() => fallback(name))
  }, [enabled, fallback])

  return { audioEnabled: enabled, unlockAudio: unlock, play }
}
