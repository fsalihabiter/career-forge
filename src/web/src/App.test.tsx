import { fireEvent, render, screen, waitFor } from '@testing-library/react'
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

function adminToken() {
  return `header.${btoa(JSON.stringify({ role: 'Administrator' }))}.signature`
}

function catalogResponse(url: string) {
  if (url.endsWith('/technologies')) return response(technologies)
  if (url.endsWith('/skills')) return response(skills)
  if (url.endsWith('/specializations')) return response(specializations)
  if (url.endsWith('/me/dashboard')) {
    return response({
      nextWork: {
        kind: 'diagnostic',
        title: 'İlk tanılamanı tamamla',
        description: 'Yetkinlik haritan için ilk kanıtları topla.',
      },
      dueReviewCount: 0,
    })
  }
  return null
}

describe('App', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.stubGlobal('scrollTo', vi.fn())
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
  }, 10_000)

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
      selfScore: 50,
      evaluation: {
        rubric: 'Teknik cevap değerlendirmesi',
        rubricVersion: 1,
        overallScore: 76.5,
        dimensions: [
          { key: 'technicalAccuracy', label: 'Teknik doğruluk', weight: 40, score: 80, feedback: 'Beklenen kanıtlardan eşleşenler: Idempotency key.' },
          { key: 'analysis', label: 'Analiz', weight: 25, score: 70, feedback: 'Bu boyut mevcut; gerekçe ve somut örnekle güçlendirilebilir.' },
        ],
        matchedSignals: ['Idempotency key'],
        matchedRedFlags: [],
      },
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
      if (url.endsWith('/review-items/question-1') && init?.method === 'POST') {
        return response({
          id: 'review-1',
          questionId: question.id,
          prompt: question.prompt,
          type: question.type,
          level: question.level,
          skillId: 'skill-1',
          skillSlug: 'api-design',
          skill: question.skill,
          technology: 'ASP.NET Core',
          addedAt: '2026-07-26T18:00:00Z',
        })
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
    expect(screen.getByText('76.5')).toBeInTheDocument()
    expect(screen.getByText('Teknik doğruluk')).toBeInTheDocument()
    expect(screen.getByText(/Öz değerlendirmen: 50/)).toBeInTheDocument()
    expect(screen.getByRole('progressbar', { name: 'Teknik doğruluk puanı' })).toHaveAttribute('aria-valuenow', '80')
    await user.click(screen.getByRole('button', { name: '+ Tekrar listesine ekle' }))
    expect(await screen.findByRole('button', { name: '✓ Tekrar listesinde' })).toBeDisabled()
  }, 10_000)

  it('filters the learning guide and opens an accessible lesson reader', async () => {
    localStorage.setItem('careerforge-token', 'existing-token')
    const learningTechnologies = [
      { id: 'tech-1', slug: 'dotnet', name: 'ASP.NET Core', category: 'Backend', accent: '#6844b8', lessonCount: 1 },
    ]
    const lesson = {
      stableId: 'middleware-order',
      version: 1,
      slug: 'middleware-order',
      title: 'Middleware sırası',
      summary: 'HTTP pipeline sırasını doğru kur.',
      level: 'intermediate',
      estimatedMinutes: 12,
      technology: technologies[0],
    }
    const pattern = { ...lesson, stableId: 'outbox', slug: 'outbox', title: 'Transactional Outbox', category: 'Dağıtık sistem' }
    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = urlOf(input)
      if (url.endsWith('/learning/technologies')) return response(learningTechnologies)
      if (url.endsWith('/learning/patterns/outbox')) return response({ ...pattern, objectives: ['Dual-write problemini açıklamak'], prerequisites: [], sections: [{ key: 'atomicity', title: 'Atomiklik sınırı', order: 1, bodyMarkdown: 'Aynı transaction içinde yaz.', codeLanguage: 'sql', codeSample: 'INSERT INTO outbox_messages ...;' }] })
      if (url.endsWith('/learning/patterns')) return response([pattern])
      if (url.includes('/learning/lessons?technology=dotnet')) return response([lesson])
      if (url.endsWith('/learning/lessons/middleware-order/progress') && init?.method === 'PUT') {
        return response({
          lessonStableId: lesson.stableId,
          lessonVersion: 1,
          lastSectionKey: 'pipeline',
          completedSectionKeys: ['pipeline'],
          completedSections: 1,
          totalSections: 1,
          completed: true,
          updatedAt: '2026-07-26T18:00:00Z',
        })
      }
      if (url.endsWith('/learning/lessons/middleware-order/progress')) {
        return response({
          lessonStableId: lesson.stableId,
          lessonVersion: 1,
          lastSectionKey: 'pipeline',
          completedSectionKeys: [],
          completedSections: 0,
          totalSections: 1,
          completed: false,
          updatedAt: '2026-07-26T18:00:00Z',
        })
      }
      if (url.endsWith('/learning/lessons/middleware-order')) {
        return response({
          ...lesson,
          objectives: ['Pipeline sırasını açıklamak'],
          prerequisites: ['Temel HTTP bilgisi'],
          sections: [
            { key: 'pipeline', title: 'Pipeline nasıl işler?', order: 1, bodyMarkdown: 'Her middleware sırayla çalışır.', codeLanguage: 'csharp', codeSample: 'app.UseAuthentication();' },
          ],
        })
      }
      if (url.endsWith('/learning/lessons')) return response([lesson])
      const catalog = catalogResponse(url)
      if (catalog) return catalog
      if (url.endsWith('/me/skills')) return response([])
      if (url.endsWith('/learning-paths/current')) return response({ items: [] })
      return response({}, 404)
    })
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<App />)
    await user.click(await screen.findByRole('button', { name: 'Rehber' }))

    expect(await screen.findByRole('heading', { name: 'Ders kataloğu' })).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /ASP.NET Core/ }))
    expect(await screen.findByText('Middleware sırası')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: /Dersi aç/ }))

    expect(await screen.findByRole('heading', { name: 'Middleware sırası' })).toBeInTheDocument()
    expect(screen.getByRole('navigation', { name: 'Ders bölümleri' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Pipeline nasıl işler?' })).toBeInTheDocument()
    expect(screen.getByText('app.UseAuthentication();')).toBeInTheDocument()
    expect(screen.getByText('0 / 1 bölüm')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Dersi tamamla' }))
    expect(await screen.findByText('DERS TAMAMLANDI')).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringMatching(/\/learning\/lessons\/middleware-order\/progress$/),
      expect.objectContaining({ method: 'PUT' }),
    )

    await user.click(screen.getByRole('button', { name: 'Rehber' }))
    await user.click(await screen.findByRole('tab', { name: 'Patternler' }))
    expect(await screen.findByText('Transactional Outbox')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: /Pattern’i aç/ }))
    expect(await screen.findByRole('heading', { name: 'Transactional Outbox' })).toBeInTheDocument()
    expect(screen.getByText('INSERT INTO outbox_messages ...;')).toBeInTheDocument()
  })

  it('schedules the next review from the recall rating', async () => {
    localStorage.setItem('careerforge-token', 'existing-token')
    const reviewItem = {
      id: 'review-1',
      questionId: 'question-1',
      prompt: 'Bir API isteğini nasıl idempotent yaparsın?',
      type: 'Tasarım',
      level: 'intermediate',
      skillId: 'skill-1',
      skillSlug: 'api-design',
      skill: 'API tasarımı',
      technology: 'ASP.NET Core',
      addedAt: '2026-07-26T18:00:00Z',
      nextReviewAt: '2026-07-26T18:00:00Z',
      intervalDays: 0,
      repetitionCount: 0,
    }
    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = urlOf(input)
      const catalog = catalogResponse(url)
      if (catalog) return catalog
      if (url.endsWith('/me/skills')) return response([])
      if (url.endsWith('/learning-paths/current')) return response({ items: [] })
      if (url.endsWith('/review-items/')) return response([reviewItem])
      if (url.endsWith('/review-items/question-1/reviews') && init?.method === 'POST') {
        return response({
          ...reviewItem,
          nextReviewAt: '2026-07-30T18:00:00Z',
          lastReviewedAt: '2026-07-27T18:00:00Z',
          intervalDays: 3,
          repetitionCount: 1,
        })
      }
      return response({}, 404)
    })
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<App />)
    await user.click(await screen.findByRole('button', { name: 'Tekrar' }))
    expect(await screen.findByText(reviewItem.prompt)).toBeInTheDocument()
    expect(screen.getByText('Bugün')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Kolay, uzun' }))

    expect(await screen.findByText('3 gün aralık')).toBeInTheDocument()
    expect(screen.getByText(/1 tekrar/)).toBeInTheDocument()
    expect(fetchMock).toHaveBeenCalledWith(
      expect.stringMatching(/\/review-items\/question-1\/reviews$/),
      expect.objectContaining({
        method: 'POST',
        body: JSON.stringify({ rating: 'easy' }),
      }),
    )
  })

  it('shows next work, weakest skill and latest evidence on the dashboard', async () => {
    localStorage.setItem('careerforge-token', 'existing-token')
    const summary = {
      nextWork: {
        kind: 'review',
        title: 'Idempotent API tasarımını yeniden kur',
        description: 'API tasarımı alanında 2 soru bugün tekrar bekliyor.',
        scheduledAt: '2026-07-30T08:00:00Z',
      },
      weakestSkill: {
        userSkillId: 'user-skill-1',
        skill: 'API tasarımı',
        technology: 'ASP.NET Core',
        measuredLevel: 'basic',
        confidenceScore: 40,
      },
      lastResult: {
        sessionId: 'session-1',
        kind: 'diagnostic',
        score: 68.5,
        answeredQuestions: 8,
        completedAt: '2026-07-29T18:00:00Z',
      },
      dueReviewCount: 2,
    }
    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = urlOf(input)
      if (url.endsWith('/me/dashboard')) return response(summary)
      const catalog = catalogResponse(url)
      if (catalog) return catalog
      if (url.endsWith('/me/skills')) return response([])
      if (url.endsWith('/learning-paths/current')) return response({ items: [] })
      return response({}, 404)
    })
    vi.stubGlobal('fetch', fetchMock)

    render(<App />)

    expect(await screen.findByText(summary.nextWork.title)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Tekrarları aç →' })).toBeInTheDocument()
    expect(screen.getByText('API tasarımı')).toBeInTheDocument()
    expect(screen.getByText('Temel · %40 güven')).toBeInTheDocument()
    expect(screen.getByText('68.5 / 100')).toBeInTheDocument()
    expect(screen.getByText('8 cevap · Tanılama')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
  })

  it('supports keyboard entry, semantic tabs and focus after screen changes', async () => {
    const fetchMock = vi.fn(async (input: RequestInfo | URL) => {
      const url = urlOf(input)
      if (url.endsWith('/learning/technologies')) return response([])
      if (url.endsWith('/learning/lessons')) return response([])
      if (url.endsWith('/learning/patterns')) return response([])
      return catalogResponse(url) ?? response({}, 404)
    })
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<App />)

    await user.tab()
    const skipLink = screen.getByRole('link', { name: 'Ana içeriğe geç' })
    expect(skipLink).toHaveFocus()
    await user.click(skipLink)
    expect(screen.getByRole('main')).toHaveFocus()

    const registerTab = screen.getByRole('tab', { name: 'Hesap oluştur' })
    const loginTab = screen.getByRole('tab', { name: 'Giriş yap' })
    expect(registerTab).toHaveAttribute('aria-selected', 'true')
    await user.click(loginTab)
    expect(loginTab).toHaveAttribute('aria-selected', 'true')

    await user.click(screen.getByRole('button', { name: 'Rehber' }))
    expect(await screen.findByRole('heading', { name: 'Ders kataloğu' })).toBeInTheDocument()
    expect(screen.getByRole('main')).toHaveFocus()
    expect(screen.getByRole('button', { name: 'Rehber' })).toHaveAttribute('aria-current', 'page')
  })

  it('lets an administrator manage every content type and create a lesson', async () => {
    localStorage.setItem('careerforge-token', adminToken())
    const lesson = {
      stableId: 'lesson-one', version: 1, slug: 'lesson-one', title: 'İlk ders',
      summary: '', technologySlug: null, level: 'intermediate', estimatedMinutes: 20,
      status: 'draft', objectives: [], prerequisites: [], category: null,
      sections: [{ key: 'intro', title: 'Giriş', order: 1, bodyMarkdown: 'İçerik', codeLanguage: null, codeSample: null }],
    }
    const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
      const url = urlOf(input)
      const catalog = catalogResponse(url)
      if (catalog) return catalog
      if (url.endsWith('/me/skills')) return response([])
      if (url.endsWith('/learning-paths/current')) return response({ items: [] })
      if (url.endsWith('/admin/content/lessons/') && init?.method === 'POST') {
        return response(JSON.parse(String(init.body)), 201)
      }
      if (url.endsWith('/admin/content/lessons/lesson-one/1')) return response(lesson)
      if (url.includes('/admin/content/')) return response(url.endsWith('/lessons/') ? [lesson] : [])
      return response({}, 404)
    })
    vi.stubGlobal('fetch', fetchMock)
    const user = userEvent.setup()

    render(<App />)
    await user.click(await screen.findByRole('button', { name: 'İçerik' }))

    expect(await screen.findByRole('heading', { name: /Bilgiyi yayına hazırla/i })).toBeInTheDocument()
    expect(screen.getAllByRole('tab')).toHaveLength(4)
    expect(await screen.findByText('İlk ders')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Yeni ders' }))
    const editor = screen.getByRole('textbox', { name: 'İçerik sözleşmesi' })
    fireEvent.change(editor, { target: { value: JSON.stringify({ ...lesson, stableId: 'lesson-two', slug: 'lesson-two', title: 'İkinci ders' }) } })
    await user.click(screen.getByRole('button', { name: 'Değişiklikleri kaydet' }))

    await waitFor(() => expect(fetchMock).toHaveBeenCalledWith(
      expect.stringContaining('/admin/content/lessons/'),
      expect.objectContaining({ method: 'POST' }),
    ))
  })
})
