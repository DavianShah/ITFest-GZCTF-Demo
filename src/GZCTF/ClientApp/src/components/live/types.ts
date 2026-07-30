export type StageSoundName =
  | 'spin' | 'categorySelected' | 'gameStart' | 'hintDrop' | 'firstBlood' | 'secondBlood' | 'thirdBlood'
  | 'correctSubmit' | 'wrongSubmit' | 'reminder' | 'countdownTick' | 'overtime' | 'roundFinished' | 'scoreUpdate'

export type AnnouncementKind =
  | 'spin' | 'category' | 'start' | 'firstBlood' | 'blood' | 'hint' | 'correct' | 'wrong'
  | 'reminder' | 'countdown' | 'overtime' | 'finished'

export interface LiveAnnouncement {
  key: string
  kind: AnnouncementKind
  title: string
  text?: string
  sound: StageSoundName
  duration?: number
}
