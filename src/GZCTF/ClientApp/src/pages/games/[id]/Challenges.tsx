import { Stack } from '@mantine/core'
import { FC } from 'react'
import { ChallengePanel } from '@Components/ChallengePanel'
import { GameNoticePanel } from '@Components/GameNoticePanel'
import { SpeedrunBanner } from '@Components/SpeedrunBanner'
import { TeamRank } from '@Components/TeamRank'
import { WithGameTab } from '@Components/WithGameTab'
import { WithNavBar } from '@Components/WithNavbar'
import { WithRole } from '@Components/WithRole'
import { Role } from '@Api'
import competition from '@Styles/Competition.module.css'

const Challenges: FC = () => {
  return (
    <WithNavBar width="90%" minWidth={0}>
      <WithRole requiredRole={Role.User}>
        <WithGameTab>
          <SpeedrunBanner />
          <div className={competition.challengeLayout}>
            <ChallengePanel />
            <Stack gap="sm" className={competition.supportRail}>
              <TeamRank />
              <GameNoticePanel />
            </Stack>
          </div>
        </WithGameTab>
      </WithRole>
    </WithNavBar>
  )
}

export default Challenges
