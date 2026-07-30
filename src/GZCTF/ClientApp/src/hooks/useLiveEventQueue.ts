import { useCallback, useEffect, useRef, useState } from 'react'
import { LiveAnnouncement } from '@Components/live/types'

export const useLiveEventQueue = (onPlay: (event: LiveAnnouncement) => void) => {
  const [active, setActive] = useState<LiveAnnouncement>()
  const queue = useRef<LiveAnnouncement[]>([])
  const seen = useRef(new Set<string>())

  const enqueue = useCallback((event: LiveAnnouncement) => {
    if (seen.current.has(event.key)) return
    seen.current.add(event.key)
    queue.current.push(event)
    setActive((current) => current ?? queue.current.shift())
  }, [])

  const markSeen = useCallback((keys: string[]) => keys.forEach((key) => seen.current.add(key)), [])

  useEffect(() => {
    if (!active) return
    onPlay(active)
    const timer = window.setTimeout(() => setActive(queue.current.shift()), active.duration ?? 3200)
    return () => window.clearTimeout(timer)
  }, [active, onPlay])

  return { active, enqueue, markSeen }
}
