import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'

const technologies = [
  { id: 'tech-1', slug: 'dotnet', name: 'ASP.NET Core', category: 'Backend', maturity: 'complete', accent: '#6844b8' },
]
const skills = [
  { id: 'skill-1', slug: 'api-design', name: 'API tasarımı', category: 'Backend', description: 'Güvenilir API sözleşmeleri' },
]
const specializations = [
  {
    id: 'spec-1',
    slug: 'backend',
    name: 'Backend Developer',
    description: 'API ve veri odaklı rota',
    skills: [{ skillId: 'skill-1', name: 'API tasarımı', required: true, weight: 100 }],
  },
]

function response(data: unknown, status = 200) {
  return new Response(status === 204 ? null : JSON.stringify(data), {
    status,
    headers: status === 204 ? undefined : { 'Content-Type': 'application/json' },
  })
}

function urlOf(input: RequestInfo | URL) {
  if (typeof input === 'string') return input
  if (input instanceof URL) return input.toString()
  return input.url
}

function catalogResponse(url: string) {
  if (url.endsWith('/technologies')) return response(technologies)
  if (url.endsWith('/skills')) return response(skills)
  if (url.endsWith('/specializations')) return response(specializations)
  return null
}

describe('App', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('renders the account entry screen and loads the public catalog', async () => {
    const fetchMock = vi.fn(async (input: RequestInfo | URL) =>
      catalogResponse(urlOf(input)) ?? response({}, 404))
    vi.stubGlobal('fetch', fetchMock)

    render(<App />)

    expect(screen.getByRole('heading', {
      name: /Mülakatta bildiğini değil, nasıl düşündüğünü göster/i,
    })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Yetkinliklerimi seç' })).toBeInTheDocument()

    await waitFor(() => {
      expect(fetchMock).toHaveBeenCalledTimes(3)
    })
  })

  it('completes onboarding from registration to the personal route', async () => {
    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = urlOf(input)
      const catalog = catalogResponse(url)
      if (catalog) return catalog
      if (url.endsWith('/auth/register') && init?.method === 'POST') {
        return response({ accessToken: 'test-token', onboardingCompleted: false })
      }
      if (url.endsWith('/me/preparation-profile') && init?.method === 'PUT') {
        return response({ onboardingCompleted: true, learningPathId: 'path-1' })
      }
      if (url.endsWith('/me/skills')) return response([])
      if (url.endsWith('/learning-paths/current')) return response({ items: [] })
      return response({}, 404)
    })
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<App />)
    await user.type(screen.getByLabelText('Adın'), 'Test Kullanıcısı')
    await user.type(screen.getByLabelText('E-posta'), 'test@careerforge.test')
    await user.type(screen.getByLabelText('Parola'), 'IntegrationPass123')
    await user.click(screen.getByRole('button', { name: 'Yetkinliklerimi seç' }))

    expect(await screen.findByRole('heading', {
      name: 'Nasıl hazırlanmak istiyorsun?',
    })).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Devam et/ }))
    expect(screen.getByRole('heading', {
      name: 'Seviyeyi yıl değil, kanıt belirlesin.',
    })).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Devam et/ }))

    await user.click(screen.getByRole('button', { name: /Backend Developer/ }))
    await user.click(screen.getByRole('button', { name: /ASP.NET Core/ }))
    await user.click(screen.getByRole('button', { name: /Devam et/ }))
    expect(screen.getByRole('heading', {
      name: 'Tanılama için başlangıç profilin hazır.',
    })).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Rotamı oluştur/ }))

    expect(await screen.findByRole('heading', {
      name: /Bugün ezber değil, bir karar çalış/,
    })).toBeInTheDocument()
    expect(localStorage.getItem('careerforge-token')).toBe('test-token')
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringMatching(/\/me\/preparation-profile$/),
      expect.objectContaining({ method: 'PUT' }),
    )
  })

  it('answers a diagnostic question and opens the completed result', async () => {
    localStorage.setItem('careerforge-token', 'existing-token')
    const question = {
      id: 'question-1',
      order: 1,
      prompt: 'Bir API isteğini nasıl idempotent yaparsın?',
      type: 'Tasarım',
      level: 'intermediate',
      skill: 'API tasarımı',
      answered: false,
    }
    const completedQuestion = {
      ...question,
      answered: true,
      modelAnswer: 'Idempotency key ve unique constraint birlikte kullanılır.',
      signals: ['Idempotency key'],
      redFlags: ['Sadece butonu kapatmak'],
    }
    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = urlOf(input)
      const catalog = catalogResponse(url)
      if (catalog) return catalog
      if (url.endsWith('/me/skills')) return response([])
      if (url.endsWith('/learning-paths/current')) return response({ items: [] })
      if (url.endsWith('/diagnostic-sessions/') && init?.method === 'POST') {
        return response({ id: 'session-1' }, 201)
      }
      if (url.endsWith('/diagnostic-sessions/session-1/answers/question-1') && init?.method === 'POST') {
        return response(null, 204)
      }
      if (url.endsWith('/diagnostic-sessions/session-1/complete') && init?.method === 'POST') {
        return response({ id: 'session-1', answered: 1, total: 1 })
      }
      if (url.endsWith('/diagnostic-sessions/session-1/result')) {
        return response({ id: 'session-1', kind: 'diagnostic', status: 'completed', questions: [completedQuestion] })
      }
      if (url.endsWith('/diagnostic-sessions/session-1')) {
        return response({ id: 'session-1', kind: 'diagnostic', status: 'active', questions: [question] })
      }
      return response({}, 404)
    })
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<App />)
    await user.click(await screen.findByRole('button', { name: /Tanılamayı başlat/ }))
    expect(await screen.findByRole('heading', {
      name: question.prompt,
    })).toBeInTheDocument()

    await user.type(screen.getByLabelText('Cevabın'), 'Idempotency key ve unique constraint kullanırım.')
    await user.click(screen.getByRole('button', { name: 'Oturumu tamamla' }))

    expect(await screen.findByRole('heading', {
      name: /Tek puan değil, kanıt haritası/,
    })).toBeInTheDocument()
    expect(screen.getByText(completedQuestion.modelAnswer)).toBeInTheDocument()
    expect(screen.getByText('Idempotency key')).toBeInTheDocument()
    expect(screen.getByText('Sadece butonu kapatmak')).toBeInTheDocument()
  })
})
