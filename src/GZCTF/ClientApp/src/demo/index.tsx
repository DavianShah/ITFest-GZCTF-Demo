import { Group, Select, Text } from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { HubConnectionBuilder } from '@microsoft/signalr'
import { AxiosError, AxiosHeaders, InternalAxiosRequestConfig } from 'axios'
import { FC, useEffect } from 'react'
import { useUser } from '@Hooks/useUser'
import api, { Role } from '@Api'
import classes from './Demo.module.css'
import { demoMutationResult, getDemoRead } from './fixtures'

export const DEMO_MODE = import.meta.env.VITE_DEMO_MODE === 'true'

const SESSION_KEY = 'hacktoday-demo-session'
const ROLE_KEY = 'hacktoday-demo-role'
const DEMO_ACTION_EVENT = 'hacktoday-demo-action'

const getRole = () => (localStorage.getItem(ROLE_KEY) as Role | null) ?? Role.Admin
const hasSession = () => localStorage.getItem(SESSION_KEY) === 'active'

const parseBody = (body: unknown) => {
  if (typeof body !== 'string') return body
  try {
    return JSON.parse(body)
  } catch {
    return body
  }
}

const response = (config: InternalAxiosRequestConfig, data: unknown) => ({
  data,
  status: 200,
  statusText: 'OK',
  headers: new AxiosHeaders({ 'x-demo-data': 'true' }),
  config,
})

const unauthorized = (config: InternalAxiosRequestConfig) => {
  const res = {
    ...response(config, { title: 'Demo login required', status: 401 }),
    status: 401,
    statusText: 'Unauthorized',
  }
  throw Object.assign(new AxiosError('Demo login required', 'ERR_BAD_REQUEST', config, undefined, res), { status: 401 })
}

const installNetworkGuards = () => {
  const announce = () => window.dispatchEvent(new Event(DEMO_ACTION_EVENT))
  const isBlocked = (value?: string | URL | null) => {
    if (!value) return false
    const url = new URL(value.toString(), window.location.origin)
    return url.pathname.startsWith('/api/') || url.pathname.startsWith('/hub/') || url.hostname.endsWith('.invalid')
  }

  const originalFetch = window.fetch.bind(window)
  window.fetch = (input, init) => {
    const value = typeof input === 'string' || input instanceof URL ? input : input.url
    if (isBlocked(value)) {
      const path = new URL(value.toString(), window.location.origin).pathname
      if (path.startsWith('/hub/')) return Promise.reject(new TypeError('Realtime is disabled in demo mode'))
      announce()
      return Promise.resolve(new Response('DEMO / MOCK DATA', { status: 200 }))
    }
    return originalFetch(input, init)
  }

  const originalOpen = window.open.bind(window)
  window.open = ((url?: string | URL, target?: string, features?: string) => {
    if (isBlocked(url)) {
      announce()
      return null
    }
    return originalOpen(url, target, features)
  }) as typeof window.open

  document.addEventListener(
    'click',
    (event) => {
      const target = event.target instanceof Element ? event.target.closest('a') : null
      if (target && isBlocked(target.href)) {
        event.preventDefault()
        announce()
      }
    },
    true
  )
}

const disableRealtime = () => {
  const build = () =>
    ({
      serverTimeoutInMilliseconds: 0,
      on: () => undefined,
      off: () => undefined,
      onclose: () => undefined,
      start: async () => undefined,
      stop: async () => undefined,
    }) as unknown as ReturnType<HubConnectionBuilder['build']>

  Object.defineProperty(HubConnectionBuilder.prototype, 'build', { value: build })
}

export const installDemoMode = () => {
  if (!DEMO_MODE) return

  api.instance.defaults.adapter = async (config) => {
    const path = new URL(config.url ?? '/', window.location.origin).pathname
    const method = config.method?.toUpperCase() ?? 'GET'
    const body = parseBody(config.data)

    if (path === '/api/account/login' && method === 'POST') {
      const credentials = body as { userName?: string; password?: string }
      if (credentials.userName !== 'admin' || credentials.password !== 'demo123') unauthorized(config)
      localStorage.setItem(SESSION_KEY, 'active')
      localStorage.setItem(ROLE_KEY, Role.Admin)
      return response(config, {})
    }

    if (path === '/api/account/logout') {
      localStorage.removeItem(SESSION_KEY)
      localStorage.removeItem(ROLE_KEY)
      return response(config, {})
    }

    if (path === '/api/account/profile') {
      if (!hasSession()) unauthorized(config)
      return response(config, {
        userId: '11111111-1111-1111-1111-111111111111',
        role: getRole(),
        userName: 'admin',
        email: 'admin@demo.invalid',
      })
    }

    if (method === 'GET') {
      if (config.responseType === 'blob') return response(config, new Blob(['DEMO / MOCK DATA']))
      const data = getDemoRead(path)
      if (data !== undefined) return response(config, data)
      console.warn(`[demo] Missing fixture for ${path}`)
      return response(config, {})
    }

    return response(config, demoMutationResult(path, body))
  }

  disableRealtime()
  installNetworkGuards()
}

export const DemoBar: FC = () => {
  const { user, mutate } = useUser()

  useEffect(() => {
    const notify = () =>
      showNotification({
        id: 'demo-local-action',
        color: 'yellow',
        title: 'DEMO / MOCK DATA',
        message: 'This action stays local and does not contact a backend service.',
      })
    window.addEventListener(DEMO_ACTION_EVENT, notify)
    return () => window.removeEventListener(DEMO_ACTION_EVENT, notify)
  }, [])

  if (!DEMO_MODE) return null

  return (
    <Group className={classes.bar} gap="xs" wrap="nowrap">
      <Text className={classes.label}>DEMO / MOCK DATA</Text>
      {user ? (
        <Select
          aria-label="Demo role"
          className={classes.role}
          allowDeselect={false}
          value={user.role}
          data={[
            { value: Role.User, label: 'Participant / User' },
            { value: Role.Monitor, label: 'Monitor' },
            { value: Role.Admin, label: 'Admin' },
          ]}
          onChange={(role) => {
            if (!role) return
            localStorage.setItem(ROLE_KEY, role)
            void mutate()
          }}
        />
      ) : (
        <Text className={classes.credentials}>LOGIN admin / demo123</Text>
      )}
    </Group>
  )
}
