const now = Date.now()

const challenges = {
  Web: [
    {
      id: 101,
      title: 'Baby SQLi',
      category: 'Web',
      score: 250,
      solved: 42,
      deadline: now + 7_200_000,
      bloods: [
        { id: 1, name: 'ZERODAY', submitTimeUtc: now - 3_000_000 },
        { id: 3, name: 'BYTEME', submitTimeUtc: now - 2_800_000 },
      ],
      disableBloodBonus: false,
    },
    {
      id: 102,
      title: 'Cookie Smuggler',
      category: 'Web',
      score: 400,
      solved: 18,
      bloods: [],
      disableBloodBonus: false,
    },
  ],
  Crypto: [
    {
      id: 103,
      title: 'XOR Magic',
      category: 'Crypto',
      score: 100,
      solved: 67,
      bloods: [],
      disableBloodBonus: false,
    },
  ],
  Pwn: [
    {
      id: 105,
      title: 'Heap of Trouble',
      category: 'Pwn',
      score: 500,
      solved: 6,
      bloods: [],
      disableBloodBonus: false,
    },
  ],
  Reverse: [
    {
      id: 106,
      title: 'Snake Oil',
      category: 'Reverse',
      score: 300,
      solved: 21,
      bloods: [],
      disableBloodBonus: false,
    },
  ],
  Misc: [
    {
      id: 107,
      title: 'Sanity Check',
      category: 'Misc',
      score: 50,
      solved: 95,
      bloods: [],
      disableBloodBonus: true,
    },
  ],
}

const challengeList = Object.values(challenges).flat()
const teamRows = [
  [1, 'ZERODAY', 10200, 1, 6],
  [2, 'CYBERKNIGHTS', 8450, 2, 5],
  [3, 'BYTEME', 7900, 3, 5],
  [4, 'PWN RANGERS', 7100, 4, 4],
  [5, 'NULLBYTE', 6850, 5, 4],
  [6, 'SCRIPTKIDDIES', 6200, 6, 3],
  [7, 'FLAGHUNTERS', 5400, 7, 3],
] as const

const scoreboardItems = teamRows.map(([id, name, score, rank, solvedCount]) => ({
  id,
  name,
  bio: 'National CTF team competing at HackToday 2026.',
  divisionId: id % 2 ? 1 : 2,
  avatar: null,
  score,
  rank,
  divisionRank: Math.ceil(rank / 2),
  lastSubmissionTime: now - id * 500_000,
  solvedChallenges: challengeList.slice(0, solvedCount).map((challenge, index) => ({
    id: challenge.id,
    score: Math.max(50, Math.floor(score / solvedCount)),
    type: ['FirstBlood', 'SecondBlood', 'ThirdBlood', 'Normal'][Math.min(rank - 1, 3)],
    userName: `player${id}`,
    time: now - id * 500_000 - index * 120_000,
  })),
  solvedCount,
}))

const divisions = [
  { id: 1, name: 'Open', defaultPermissions: 2147483647, challengeConfigs: {} },
  { id: 2, name: 'University', defaultPermissions: 2147483647, challengeConfigs: {} },
]

const game = {
  id: 1,
  title: 'HackToday Main Event',
  summary: 'The flagship national capture-the-flag competition.',
  content: '## Competition systems are online\n\nReview the rules, join your team, and enter the challenge board.',
  hidden: false,
  divisions: divisions.map(({ id, name }) => ({ id, name, inviteCodeRequired: false })),
  inviteCodeRequired: false,
  writeupRequired: true,
  writeupDeadline: now + 86_400_000,
  writeupNote: 'Submit one PDF per team. Demo uploads remain in this browser only.',
  practiceMode: true,
  mode: 'Speedrun',
  status: 'Accepted',
  teamName: 'CYBERKNIGHTS',
  teamCount: 24,
  limit: 4,
  start: now - 21_600_000,
  end: now - 3_600_000,
}

const games = [
  game,
  {
    id: 2,
    title: 'University Qualifier',
    summary: 'Qualifying round for student teams across Indonesia.',
    poster: null,
    limit: 4,
    start: now + 86_400_000,
    end: now + 108_000_000,
  },
  {
    id: 3,
    title: 'Open Practice Arena',
    summary: 'Warm up before the competition goes live.',
    poster: null,
    limit: 0,
    start: now - 172_800_000,
    end: now - 86_400_000,
  },
]

const scoreboard = {
  updateTimeUtc: now,
  bloodBonus: 10,
  timelines: [
    {
      divisionId: 0,
      teams: teamRows.slice(0, 5).map(([id, name, score]) => ({
        id,
        name,
        items: [
          { time: now - 3_000_000, score: Math.floor(score / 3) },
          { time: now - 1_500_000, score: Math.floor((score * 2) / 3) },
          { time: now - 300_000, score },
        ],
      })),
    },
  ],
  items: scoreboardItems,
  divisions,
  challenges,
  challengeCount: challengeList.length,
}

const posts = [
  {
    id: 'competition-online',
    title: 'Competition systems are online',
    summary: 'Registration is open. Review the rules and prepare your team before entering the arena.',
    content:
      '## HackToday is live\n\nThe event board, challenge services, and scoreboard are ready for competition day.',
    isPinned: true,
    tags: ['announcement', 'competition'],
    authorName: 'HackToday Committee',
    time: now - 3_600_000,
  },
  {
    id: 'technical-briefing',
    title: 'Technical briefing published',
    summary: 'The participant briefing and event schedule are now available.',
    content: '## Technical briefing\n\nKeep all activity inside the provided competition scope.',
    isPinned: false,
    tags: ['briefing'],
    authorName: 'IT TODAY 2026',
    time: now - 7_200_000,
  },
]

const challengeDetails = Object.fromEntries(
  challengeList.map((challenge) => [
    challenge.id,
    {
      ...challenge,
      content:
        challenge.id === 101
          ? '## Briefing\n\nThe admin portal trusts more than it should. Recover the flag without damaging the service.\n\n> Scope is limited to the provided demo target.'
          : '## Briefing\n\nAnalyze the supplied material and recover the HackToday flag.',
      hints: challenge.id === 101 ? ['Start with the numeric identifier.', 'Observe how errors change.'] : [],
      type: challenge.id === 107 ? 'StaticAttachment' : challenge.id === 101 ? 'DynamicContainer' : 'StaticAttachment',
      context: {
        closeTime: challenge.id === 101 ? now + 3_600_000 : null,
        instanceEntry: challenge.id === 101 ? 'demo-target.invalid:31337' : null,
        url: '/api/demo/attachment',
        fileSize: 4096,
      },
      limit: 5,
      attempts: challenge.id === 101 ? 1 : 0,
      requireSolverUpload: false,
    },
  ])
)

const editGame = {
  ...game,
  acceptWithoutReview: true,
  whitelistOnly: false,
  teamMemberCountLimit: 4,
  containerCountLimit: 2,
  bloodBonus: 10,
  speedrunDefaultRoundDurationSeconds: 1800,
  speedrunOvertimeSeconds: 300,
}

const editChallenges = challengeList.map((challenge) => ({
  ...challenge,
  type: challenge.id === 101 ? 'DynamicContainer' : 'StaticAttachment',
  isEnabled: true,
  minScore: Math.max(25, Math.floor(challenge.score / 4)),
  originalScore: Math.max(500, challenge.score),
  deadlineUtc: 'deadline' in challenge ? challenge.deadline : undefined,
}))

const editChallenge = {
  ...editChallenges[0],
  content: 'The admin portal trusts more than it should. Recover the flag without damaging the service.',
  hints: ['Start with the numeric identifier.', 'Observe how errors change.'],
  speedrunHintReleaseSeconds: [300, 600],
  flagTemplate: 'HT26{[GUID]}',
  acceptedCount: 42,
  attachment: { id: 1, type: 'Local', url: '/api/demo/attachment', fileSize: 4096 },
  testContainer: null,
  flags: [{ id: 1, flag: 'HT26{demo_flag}' }],
  containerImage: 'hacktoday/demo-challenge:latest',
  memoryLimit: 256,
  cpuCount: 2,
  storageLimit: 256,
  exposePort: 8080,
  networkMode: 'Isolated',
  enableTrafficCapture: true,
  disableBloodBonus: false,
  requireSolverUpload: false,
  submissionLimit: 20,
  minScoreRate: 0.25,
  difficulty: 5,
}

const speedrun = {
  defaultRoundDurationSeconds: 1800,
  overtimeSeconds: 300,
  allowManualExtend: true,
  hideInactiveChallenges: true,
  emergencyHintEnabled: true,
  emergencyHintText: 'Emergency hint window is now active.',
  state: {
    isSpeedrun: true,
    currentRound: {
      id: 7,
      category: 'Web',
      status: 'Running',
      startedAtUtc: now - 420_000,
      endsAtUtc: now + 1_380_000,
      timeLeftSeconds: 1380,
      isOvertime: false,
    },
    usedCategories: ['Crypto'],
    remainingCategories: ['Web', 'Pwn', 'Reverse', 'Misc'],
  },
  categories: [
    { id: 1, category: 'Web', used: false, included: true },
    { id: 2, category: 'Pwn', used: false, included: true },
    { id: 3, category: 'Crypto', used: true, included: true },
    { id: 4, category: 'Reverse', used: false, included: false },
    { id: 5, category: 'Misc', used: false, included: true },
  ],
}

const members = [
  {
    userId: '11111111-1111-1111-1111-111111111111',
    userName: 'admin',
    email: 'admin@demo.invalid',
    role: 'Admin',
  },
  {
    userId: '22222222-2222-2222-2222-222222222222',
    userName: 'packetstorm',
    email: 'packetstorm@demo.invalid',
    role: 'User',
  },
]

export const demoReads: Record<string, unknown> = {
  '/api/config': {
    title: 'HACKTODAY',
    slogan: 'Shred the code. Own the system.',
    footerInfo: 'IT TODAY 2026 · DEMO DATA',
    customTheme: null,
    apiPublicKey: null,
    logoUrl: null,
    portMapping: 'Default',
    defaultLifetime: 120,
    extensionDuration: 30,
    renewalWindow: 10,
  },
  '/api/captcha': { type: 'None' },
  '/api/posts/latest': posts,
  '/api/posts': posts,
  '/api/game/recent': games,
  '/api/game': { data: games, length: games.length, total: games.length },
  '/api/game/1': game,
  '/api/game/1/details': {
    challenges,
    challengeCount: challengeList.length,
    rank: {
      ...scoreboardItems[1],
      solvedChallenges: scoreboardItems[1].solvedChallenges.slice(1),
      solvedCount: scoreboardItems[1].solvedCount - 1,
    },
    teamToken: 'DEMO-TEAM-TOKEN',
    writeupRequired: true,
    writeupDeadline: now + 86_400_000,
  },
  '/api/game/1/notices': [
    { id: 1, type: 'Normal', time: now - 900_000, values: ['Competition systems online.'] },
    { id: 2, type: 'FirstBlood', time: now - 700_000, values: ['ZERODAY', 'Baby SQLi'] },
    { id: 3, type: 'NewHint', time: now - 300_000, values: ['Heap of Trouble'] },
  ],
  '/api/game/1/scoreboard': scoreboard,
  '/api/game/1/speedrun/state': { ...speedrun.state, message: 'Waiting for the next category spin.' },
  '/api/game/1/writeup': {
    submitted: true,
    name: 'cyberknights-writeup.pdf',
    fileSize: 482_000,
    note: game.writeupNote,
  },
  '/api/game/1/events': [
    {
      type: 'FlagSubmit',
      values: ['Accepted', 'HT26{redacted}', 'Baby SQLi', '101'],
      time: now - 40_000,
      user: 'admin',
      team: 'CYBERKNIGHTS',
    },
    {
      type: 'CheatDetected',
      values: ['Heap of Trouble', 'NULLBYTE', 'CYBERKNIGHTS'],
      time: now - 95_000,
      user: 'sentinel',
      team: 'NULLBYTE',
    },
    {
      type: 'ContainerStart',
      values: ['demo-instance', 'Heap of Trouble'],
      time: now - 140_000,
      user: 'packetstorm',
      team: 'CYBERKNIGHTS',
    },
  ],
  '/api/game/1/submissions': [
    {
      answer: 'HT26{redacted_correct_flag}',
      aiUsageDisclosure: 'Saya tidak memakai AI',
      status: 'Accepted',
      time: now - 35_000,
      user: 'admin',
      team: 'CYBERKNIGHTS',
      challenge: 'Baby SQLi',
    },
    {
      answer: 'HT26{almost_there}',
      aiUsageDisclosure: 'https://example.invalid/demo-disclosure',
      status: 'WrongAnswer',
      time: now - 78_000,
      user: 'packetstorm',
      team: 'CYBERKNIGHTS',
      challenge: 'Heap of Trouble',
    },
  ],
  '/api/game/1/cheatinfo': [
    {
      ownedTeam: {
        id: 1,
        team: { id: 2, name: 'CYBERKNIGHTS', avatar: null },
        status: 'Accepted',
        division: 'University',
        divisionId: 2,
      },
      submitTeam: {
        id: 2,
        team: { id: 5, name: 'NULLBYTE', avatar: null },
        status: 'Suspended',
        division: 'Open',
        divisionId: 1,
      },
      submission: {
        answer: 'HT26{shared_demo_flag}',
        status: 'CheatDetected',
        time: now - 125_000,
        user: 'nullrunner',
        team: 'NULLBYTE',
        challenge: 'XOR Magic',
      },
    },
  ],
  '/api/game/games/1/captures': [
    { id: 101, title: 'Baby SQLi', category: 'Web', type: 'DynamicContainer', isEnabled: true, count: 3 },
    { id: 105, title: 'Heap of Trouble', category: 'Pwn', type: 'DynamicContainer', isEnabled: true, count: 2 },
  ],
  '/api/game/captures/101': [
    { id: 1, teamId: 2, name: 'CYBERKNIGHTS', division: 'University', avatar: null, count: 2 },
    { id: 2, teamId: 5, name: 'NULLBYTE', division: 'Open', avatar: null, count: 1 },
  ],
  '/api/game/captures/105': [
    { id: 1, teamId: 2, name: 'CYBERKNIGHTS', division: 'University', avatar: null, count: 2 },
  ],
  '/api/game/captures/101/1': [
    { fileName: '2026-competition-start.pcap', size: 24_576, updateTime: now - 60_000 },
    { fileName: '2026-follow-up.pcap', size: 8192, updateTime: now - 120_000 },
  ],
  '/api/game/captures/101/2': [{ fileName: '2026-nullbyte.pcap', size: 12_288, updateTime: now - 90_000 }],
  '/api/game/captures/105/1': [{ fileName: '2026-heap.pcap', size: 32_768, updateTime: now - 80_000 }],
  '/api/team': [
    {
      id: 2,
      name: 'CYBERKNIGHTS',
      bio: 'Breaking systems, building trust.',
      locked: false,
      members: [
        { id: members[0].userId, userName: 'admin', captain: true },
        { id: members[1].userId, userName: 'packetstorm', captain: false },
      ],
    },
    {
      id: 5,
      name: 'NULLBYTE',
      bio: 'University security research collective.',
      locked: true,
      members: [{ id: members[0].userId, userName: 'admin', captain: false }],
    },
  ],
  '/api/edit/games': { data: [editGame], length: 1, total: 1 },
  '/api/edit/games/1': editGame,
  '/api/edit/games/1/speedrun': speedrun,
  '/api/edit/games/1/challenges': editChallenges,
  '/api/edit/games/1/challenges/101': editChallenge,
  '/api/edit/games/1/challenges/105': { ...editChallenge, ...editChallenges.find(({ id }) => id === 105) },
  '/api/edit/games/1/divisions': divisions.map((division) => ({
    ...division,
    challengeConfigs: challengeList.map(({ id }) => ({ challengeId: id, permissions: 2147483647 })),
  })),
  '/api/game/1/participations': [
    {
      id: 1,
      team: {
        id: 2,
        name: 'CYBERKNIGHTS',
        bio: 'Breaking systems, building trust.',
        captainId: members[0].userId,
        members,
      },
      registeredMembers: members.map(({ userId }) => userId),
      divisionId: 2,
      status: 'Accepted',
    },
    {
      id: 2,
      team: { id: 5, name: 'NULLBYTE', bio: 'University security research collective.', members: [members[1]] },
      registeredMembers: [members[1].userId],
      divisionId: 1,
      status: 'Pending',
    },
  ],
  '/api/admin/writeups/1': {
    divisions: { 1: 'Open', 2: 'University' },
    writeups: [
      {
        id: 1,
        team: { id: 2, name: 'CYBERKNIGHTS', avatar: null },
        url: 'data:application/pdf;base64,JVBERi0xLjQKMSAwIG9iago8PCAvVHlwZSAvQ2F0YWxvZyAvUGFnZXMgMiAwIFIgPj4KZW5kb2JqCjIgMCBvYmoKPDwgL1R5cGUgL1BhZ2VzIC9LaWRzIFszIDAgUl0gL0NvdW50IDEgPj4KZW5kb2JqCjMgMCBvYmoKPDwgL1R5cGUgL1BhcmVudCAyIDAgUiAvTWVkaWFCb3ggWzAgMCA2MTIgNzkyXSAvUmVzb3VyY2VzIDw8IC9Gb250IDw8IC9GMSA1IDAgUiA+PiA+PiAvQ29udGVudHMgNCAwIFIgPj4KZW5kb2JqCjQgMCBvYmoKPDwgL0xlbmd0aCAxMDYgPj4Kc3RyZWFtCkJUIC9GMSAyNCBUZiA3MiA3MjAgVGQgKEhBQ0tUT0RBWSBERU1PIFdSSVRFVVApIFRqIDAgLTM2IFRkIC9GMSAxMiBUZiAoTW9jayBkYXRhIC0gbm8gYmFja2VuZCBmaWxlLikgVGogRVQKZW5kc3RyZWFtCmVuZG9iago1IDAgb2JqCjw8IC9UeXBlIC9Gb250IC9TdWJ0eXBlIC9UeXBlMSAvQmFzZUZvbnQgL0hlbHZldGljYSA+PgplbmRvYmoKeHJlZgowIDYKMDAwMDAwMDAwMCA2NTUzNSBmIAowMDAwMDAwMDA5IDAwMDAwIG4gCjAwMDAwMDAwNTggMDAwMDAgbiAKMDAwMDAwMDExNSAwMDAwMCBuIAowMDAwMDAwMjQxIDAwMDAwIG4gCjAwMDAwMDAzOTggMDAwMDAgbiAKdHJhaWxlcgo8PCAvU2l6ZSA2IC9Sb290IDEgMCBSID4+CnN0YXJ0eHJlZgo0NjgKJSVFT0YK',
        uploadTimeUtc: now - 1_800_000,
        divisionId: 2,
      },
    ],
  },
  '/api/admin/users': {
    data: members.map((member, index) => ({
      id: member.userId,
      userName: member.userName,
      realName: index ? 'Packet Storm' : 'Demo Administrator',
      stdNumber: `HT260${index + 1}`,
      phone: '+62 000 0000 0000',
      bio: 'HackToday demo account.',
      registerTimeUtc: now - 86_400_000,
      lastVisitedUtc: now - index * 600_000,
      ip: '192.0.2.10',
      email: member.email,
      avatar: null,
      role: member.role,
      emailConfirmed: true,
    })),
    length: members.length,
    total: members.length,
  },
  '/api/admin/config': {
    globalConfig: {
      title: 'HACKTODAY',
      slogan: 'Shred the code. Own the system.',
      description: 'National cybersecurity capture-the-flag competition.',
      footerInfo: 'IT TODAY 2026',
      customTheme: '#cf001c',
      apiEncryption: false,
      logoHash: '',
      faviconHash: '',
    },
    accountPolicy: {
      allowRegister: true,
      activeOnRegister: true,
      useCaptcha: false,
      emailConfirmationRequired: false,
      emailDomainList: 'demo.invalid',
    },
    containerPolicy: {
      autoDestroyOnLimitReached: true,
      maxExerciseContainerCountPerUser: 2,
      defaultLifetime: 120,
      extensionDuration: 30,
      renewalWindow: 10,
    },
  },
}

export const getDemoRead = (path: string) => {
  if (demoReads[path] !== undefined) return demoReads[path]

  if (/\/challenges\/\d+\/status\/\d+$/.test(path)) return 'Accepted'

  if (path.startsWith('/api/posts/')) {
    const id = path.slice('/api/posts/'.length)
    return posts.find((post) => post.id === id) ?? posts[0]
  }

  if (/^\/api\/game\/1\/challenges\/\d+$/.test(path)) {
    const id = Number(path.split('/').pop())
    return challengeDetails[id] ?? challengeDetails[101]
  }

  return undefined
}

export const demoMutationResult = (path: string, body: unknown) => {
  if (/\/container\/\d+(?:\/extend)?$/.test(path)) {
    return { entry: 'demo-target.invalid:31337', expectStopAt: Date.now() + 3_600_000 }
  }
  if (/\/challenges\/\d+\/withsolver$/.test(path) || /\/challenges\/\d+$/.test(path)) return 9001
  if (/\/status\/\d+$/.test(path)) return 'Accepted'
  if (/\/users\/[^/]+\/password$/.test(path)) return 'demo-reset-only'
  if (path === '/api/edit/games/import') return 1
  if (path.includes('/speedrun')) return speedrun
  if (/\/edit\/games\/1\/challenges\/\d+$/.test(path)) return body || editChallenge
  return body ?? {}
}
