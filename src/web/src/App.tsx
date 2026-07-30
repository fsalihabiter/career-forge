import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'
import { AdminContent } from './AdminContent'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api'

type Level = 'beginner' | 'basic' | 'intermediate' | 'advanced' | 'expert'
type Source = 'skills' | 'specialization' | 'jobRequirements' | 'general'
type Screen = 'auth' | 'onboarding' | 'dashboard' | 'learning' | 'lesson' | 'session' | 'result' | 'review' | 'admin'
type Technology = { id: string; slug: string; name: string; category: string; maturity: string; accent: string }
type Skill = { id: string; slug: string; name: string; category: string; description: string }
type Specialization = {
  id: string
  slug: string
  name: string
  description: string
  skills: { skillId: string; name: string; required: boolean; weight: number }[]
}
type UserSkill = {
  id?: string
  skillId: string
  skill?: string
  technologyId?: string
  technology?: string
  selfAssessedLevel?: string
  measuredLevel?: string
  targetLevel: string
  confidenceScore?: number
}
type SessionQuestion = {
  id: string
  order: number
  prompt: string
  type: string
  level: string
  skill: string
  technology?: string
  answered: boolean
  modelAnswer?: string
  signals?: string[]
  redFlags?: string[]
  selfScore?: number
  evaluation?: {
    rubric: string
    rubricVersion: number
    overallScore: number
    dimensions: { key: string; label: string; weight: number; score: number; feedback: string }[]
    matchedSignals: string[]
    matchedRedFlags: string[]
  }
}
type SessionData = { id: string; kind: string; status: string; questions: SessionQuestion[] }
type PathItem = { id: string; title: string; reason: string; order: number; completed: boolean }
type LearningTechnology = Pick<Technology, 'id' | 'slug' | 'name' | 'category' | 'accent'> & { lessonCount: number }
type LessonSummary = {
  stableId: string
  version: number
  slug: string
  title: string
  summary: string
  level: string
  estimatedMinutes: number
  technology?: Technology
}
type LessonSection = {
  key: string
  title: string
  order: number
  bodyMarkdown: string
  codeLanguage?: string
  codeSample?: string
}
type LessonDetail = LessonSummary & {
  objectives: string[]
  prerequisites: string[]
  sections: LessonSection[]
}
type PatternSummary = LessonSummary & { category: string }
type PatternDetail = LessonDetail & { category: string }
type LessonProgress = {
  lessonStableId: string
  lessonVersion: number
  lastSectionKey: string
  completedSectionKeys: string[]
  completedSections: number
  totalSections: number
  completed: boolean
  updatedAt: string
}
type ReviewItem = {
  id: string
  questionId: string
  prompt: string
  type: string
  level: string
  skillId: string
  skillSlug: string
  skill: string
  technology?: string
  addedAt: string
  nextReviewAt: string
  lastReviewedAt?: string
  intervalDays: number
  repetitionCount: number
}
type DashboardSummary = {
  nextWork: { kind: 'review' | 'path' | 'diagnostic'; title: string; description: string; scheduledAt?: string }
  weakestSkill?: {
    userSkillId: string
    skill: string
    technology?: string
    measuredLevel?: string
    confidenceScore: number
  }
  lastResult?: {
    sessionId: string
    kind: string
    score: number
    answeredQuestions: number
    completedAt: string
  }
  dueReviewCount: number
}

const levelLabels: Record<string, string> = {
  beginner: 'Başlangıç',
  basic: 'Temel',
  intermediate: 'Orta',
  advanced: 'İleri',
  expert: 'Uzman',
}

const sources: { id: Source; label: string; copy: string }[] = [
  { id: 'skills', label: 'Yetkinliklerimle', copy: 'Bildiklerini işaretle, eksiklerini birlikte ölçelim.' },
  { id: 'specialization', label: 'Uzmanlık seçerek', copy: 'Hazır bir kariyer rotasından başlayıp teknolojilerini belirle.' },
  { id: 'jobRequirements', label: 'İlan hedefiyle', copy: 'İlandaki gereksinimleri elle seçerek odaklı bir plan kur.' },
  { id: 'general', label: 'Senior hazırlığı', copy: 'Ortak mühendislik ve sistem tasarımı alanlarında çalış.' },
]

async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = localStorage.getItem('careerforge-token')
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  })
  if (!response.ok) {
    const body = await response.json().catch(() => null)
    const validationMessage = body?.errors
      ? Object.values(body.errors as Record<string, string[]>).flat().join(' ')
      : null
    throw new Error(body?.detail ?? validationMessage ?? body?.title ?? 'İşlem tamamlanamadı.')
  }
  if (response.status === 204) return undefined as T
  return response.json()
}

function tokenRoles() {
  const token = localStorage.getItem('careerforge-token')
  if (!token) return [] as string[]
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
    const role = payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
    return Array.isArray(role) ? role : [role].filter(Boolean)
  } catch {
    return []
  }
}

function App() {
  const [screen, setScreen] = useState<Screen>(localStorage.getItem('careerforge-token') ? 'dashboard' : 'auth')
  const authenticated = Boolean(localStorage.getItem('careerforge-token'))
  const roles = tokenRoles()
  const administrator = roles.includes('Administrator')
  const contentManager = administrator || roles.includes('ContentEditor')
  const [catalog, setCatalog] = useState<{ technologies: Technology[]; skills: Skill[]; specializations: Specialization[] }>({
    technologies: [], skills: [], specializations: [],
  })
  const [userSkills, setUserSkills] = useState<UserSkill[]>([])
  const [path, setPath] = useState<PathItem[]>([])
  const [session, setSession] = useState<SessionData | null>(null)
  const [result, setResult] = useState<SessionData | null>(null)
  const [learningTechnologies, setLearningTechnologies] = useState<LearningTechnology[]>([])
  const [lessons, setLessons] = useState<LessonSummary[]>([])
  const [lesson, setLesson] = useState<LessonDetail | null>(null)
  const [lessonProgress, setLessonProgress] = useState<LessonProgress | null>(null)
  const [patterns, setPatterns] = useState<PatternSummary[]>([])
  const [reviewItems, setReviewItems] = useState<ReviewItem[]>([])
  const [dashboard, setDashboard] = useState<DashboardSummary | null>(null)
  const [guideMode, setGuideMode] = useState<'lessons' | 'patterns'>('lessons')
  const [learningTechnology, setLearningTechnology] = useState('')
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)
  const previousScreen = useRef(screen)

  const loadCatalog = async () => {
    const [technologies, skills, specializations] = await Promise.all([
      api<Technology[]>('/technologies'),
      api<Skill[]>('/skills'),
      api<Specialization[]>('/specializations'),
    ])
    setCatalog({ technologies, skills, specializations })
  }

  const loadDashboard = async () => {
    try {
      const [skills, learningPath, summary] = await Promise.all([
        api<UserSkill[]>('/me/skills'),
        api<{ items: PathItem[] }>('/learning-paths/current').catch(() => ({ items: [] })),
        api<DashboardSummary>('/me/dashboard'),
      ])
      setUserSkills(skills)
      setPath(learningPath.items)
      setDashboard(summary)
    } catch {
      localStorage.removeItem('careerforge-token')
      setScreen('auth')
    }
  }

  useEffect(() => {
    loadCatalog().catch(() => setMessage('Katalog yüklenemedi. API servisinin çalıştığını kontrol edin.'))
    if (localStorage.getItem('careerforge-token')) loadDashboard()
  }, [])

  useEffect(() => {
    if (previousScreen.current !== screen) {
      document.getElementById('main-content')?.focus()
      previousScreen.current = screen
    }
  }, [screen])

  const startSession = async (kind: 'diagnostic' | 'interview') => {
    setLoading(true)
    setMessage('')
    try {
      const created = await api<{ id: string }>(`/${kind === 'diagnostic' ? 'diagnostic-sessions' : 'interview-sessions'}/`, {
        method: 'POST', body: JSON.stringify({ questionCount: kind === 'diagnostic' ? 8 : 10 }),
      })
      const data = await api<SessionData>(`/${kind === 'diagnostic' ? 'diagnostic-sessions' : 'interview-sessions'}/${created.id}`)
      setSession(data)
      setScreen('session')
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'Oturum oluşturulamadı.')
    } finally {
      setLoading(false)
    }
  }

  const logout = () => {
    localStorage.removeItem('careerforge-token')
    setScreen('auth')
  }

  const openLearning = async (technology = learningTechnology) => {
    setScreen('learning')
    setLoading(true)
    setMessage('')
    try {
      const query = technology ? `?technology=${encodeURIComponent(technology)}` : ''
      const [technologies, lessonList, patternList] = await Promise.all([
        api<LearningTechnology[]>('/learning/technologies'),
        api<LessonSummary[]>(`/learning/lessons${query}`),
        api<PatternSummary[]>('/learning/patterns'),
      ])
      setLearningTechnologies(technologies)
      setLessons(lessonList)
      setPatterns(patternList)
      setLearningTechnology(technology)
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'Öğrenme rehberi yüklenemedi.')
    } finally {
      setLoading(false)
    }
  }

  const openPattern = async (slug: string) => {
    setLoading(true)
    setMessage('')
    try {
      setLesson(await api<PatternDetail>(`/learning/patterns/${slug}`))
      setLessonProgress(null)
      setScreen('lesson')
      window.scrollTo({ top: 0, behavior: 'smooth' })
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'Pattern açılamadı.')
    } finally {
      setLoading(false)
    }
  }

  const openLesson = async (slug: string) => {
    setLoading(true)
    setMessage('')
    try {
      const [detail, progress] = await Promise.all([
        api<LessonDetail>(`/learning/lessons/${slug}`),
        authenticated
          ? api<LessonProgress>(`/learning/lessons/${slug}/progress`)
          : Promise.resolve(null),
      ])
      setLesson(detail)
      setLessonProgress(progress)
      setScreen('lesson')
      window.scrollTo({ top: 0, behavior: 'smooth' })
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'Ders açılamadı.')
    } finally {
      setLoading(false)
    }
  }

  const saveLessonProgress = async (lastSectionKey: string, completedSectionKeys: string[]) => {
    if (!lesson) return
    try {
      const progress = await api<LessonProgress>(`/learning/lessons/${lesson.slug}/progress`, {
        method: 'PUT',
        body: JSON.stringify({ lastSectionKey, completedSectionKeys }),
      })
      setLessonProgress(progress)
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'Ders ilerlemesi kaydedilemedi.')
      throw error
    }
  }

  const openReview = async () => {
    setScreen('review')
    setLoading(true)
    setMessage('')
    try {
      setReviewItems(await api<ReviewItem[]>('/review-items/'))
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'Tekrar listesi yüklenemedi.')
    } finally {
      setLoading(false)
    }
  }

  const addReviewItem = async (questionId: string) => {
    const item = await api<ReviewItem>(`/review-items/${questionId}`, { method: 'POST' })
    setReviewItems(current => current.some(x => x.questionId === questionId) ? current : [item, ...current])
  }

  const removeReviewItem = async (questionId: string) => {
    await api(`/review-items/${questionId}`, { method: 'DELETE' })
    setReviewItems(current => current.filter(x => x.questionId !== questionId))
  }

  const completeReviewItem = async (questionId: string, rating: string) => {
    const item = await api<ReviewItem>(`/review-items/${questionId}/reviews`, {
      method: 'POST',
      body: JSON.stringify({ rating }),
    })
    setReviewItems(current => current.map(x => x.questionId === questionId ? item : x))
  }

  return (
    <div className="app-shell">
      <a className="skip-link" href="#main-content" onClick={() => document.getElementById('main-content')?.focus()}>Ana içeriğe geç</a>
      <header className="topbar">
        <button className="brand" onClick={() => setScreen(authenticated ? 'dashboard' : 'auth')} aria-label="CareerForge ana sayfa">
          <span className="brand-mark">CF</span>
          <span><strong>CareerForge</strong><small>Mülakat çalışma sistemi</small></span>
        </button>
        <nav aria-label="Ana menü">
          {authenticated && <button aria-current={screen === 'dashboard' ? 'page' : undefined} className={screen === 'dashboard' ? 'nav-active' : ''} onClick={() => { setScreen('dashboard'); loadDashboard() }}>Rotam</button>}
          <button aria-current={screen === 'learning' || screen === 'lesson' ? 'page' : undefined} className={screen === 'learning' || screen === 'lesson' ? 'nav-active' : ''} onClick={() => openLearning()}>Rehber</button>
          {authenticated && <button aria-current={screen === 'review' ? 'page' : undefined} className={screen === 'review' ? 'nav-active' : ''} onClick={openReview}>Tekrar</button>}
          {contentManager && <button aria-current={screen === 'admin' ? 'page' : undefined} className={screen === 'admin' ? 'nav-active' : ''} onClick={() => setScreen('admin')}>İçerik</button>}
          {authenticated && <button onClick={() => startSession('interview')}>Mülakat</button>}
          {authenticated && <button onClick={logout}>Çıkış</button>}
          {!authenticated && screen !== 'auth' && <button onClick={() => setScreen('auth')}>Giriş yap</button>}
        </nav>
      </header>

      {message && <div className="notice" role="status">{message}<button onClick={() => setMessage('')}>Kapat</button></div>}

      <main id="main-content" tabIndex={-1}>
        {screen === 'auth' && <AuthScreen onAuthenticated={(onboarded) => setScreen(onboarded ? 'dashboard' : 'onboarding')} />}
        {screen === 'onboarding' && (
          <Onboarding
            catalog={catalog}
            onDone={() => { setScreen('dashboard'); loadDashboard() }}
            onError={setMessage}
          />
        )}
        {screen === 'dashboard' && (
          <Dashboard
            skills={userSkills}
            path={path}
            summary={dashboard}
            technologies={catalog.technologies}
            onDiagnostic={() => startSession('diagnostic')}
            onInterview={() => startSession('interview')}
            onReview={openReview}
            onLearning={() => openLearning()}
            onEdit={() => setScreen('onboarding')}
            loading={loading}
          />
        )}
        {screen === 'learning' && (
          <LearningGuide
            technologies={learningTechnologies}
            lessons={lessons}
            patterns={patterns}
            mode={guideMode}
            selectedTechnology={learningTechnology}
            loading={loading}
            onMode={setGuideMode}
            onTechnology={openLearning}
            onLesson={openLesson}
            onPattern={openPattern}
          />
        )}
        {screen === 'lesson' && lesson && (
          <LessonReader
            lesson={lesson}
            progress={lessonProgress}
            canTrack={authenticated && !('category' in lesson)}
            onProgress={saveLessonProgress}
            onBack={() => openLearning(learningTechnology)}
          />
        )}
        {screen === 'session' && session && (
          <SessionScreen
            session={session}
            onCancel={() => setScreen('dashboard')}
            onComplete={(data) => { setResult(data); setScreen('result') }}
            onError={setMessage}
          />
        )}
        {screen === 'result' && result && (
          <ResultScreen
            result={result}
            onAddReview={addReviewItem}
            onDone={() => { setScreen('dashboard'); loadDashboard() }}
          />
        )}
        {screen === 'review' && (
          <ReviewScreen
            items={reviewItems}
            loading={loading}
            onRemove={removeReviewItem}
            onReview={completeReviewItem}
            onPractice={() => startSession('interview')}
          />
        )}
        {screen === 'admin' && contentManager && <AdminContent request={api} onMessage={setMessage} canPublish={administrator} />}
      </main>
    </div>
  )
}

function LearningGuide({ technologies, lessons, patterns, mode, selectedTechnology, loading, onMode, onTechnology, onLesson, onPattern }: {
  technologies: LearningTechnology[]
  lessons: LessonSummary[]
  patterns: PatternSummary[]
  mode: 'lessons' | 'patterns'
  selectedTechnology: string
  loading: boolean
  onMode: (mode: 'lessons' | 'patterns') => void
  onTechnology: (slug?: string) => void
  onLesson: (slug: string) => void
  onPattern: (slug: string) => void
}) {
  const totalLessons = technologies.reduce((sum, technology) => sum + technology.lessonCount, 0)
  return (
    <section className="learning-guide">
      <header className="guide-hero">
        <div>
          <span className="eyebrow">ÖĞRENME REHBERİ</span>
          <h1>Kavramı oku.<br /><em>Kararı savun.</em></h1>
        </div>
        <p>Her ders, mülakatta ezberlenecek bir cevap yerine gerçek bir mühendislik kararını parçalar: bağlam, uygulama ve trade-off.</p>
      </header>
      <div className="guide-switch" role="tablist" aria-label="Rehber türü">
        <button role="tab" aria-selected={mode === 'lessons'} onClick={() => onMode('lessons')}>Dersler</button>
        <button role="tab" aria-selected={mode === 'patterns'} onClick={() => onMode('patterns')}>Patternler</button>
      </div>

      <div className="guide-layout">
        {mode === 'lessons' ? <aside className="guide-filter" aria-label="Teknoloji filtresi">
          <div className="filter-heading"><span>TEKNOLOJİ</span><b>{totalLessons} ders</b></div>
          <button className={!selectedTechnology ? 'active' : ''} onClick={() => onTechnology('')}>
            <i className="all-tech-mark" />Tüm dersler
            <span>{totalLessons}</span>
          </button>
          {technologies.map(technology => (
            <button
              key={technology.id}
              className={selectedTechnology === technology.slug ? 'active' : ''}
              onClick={() => onTechnology(technology.slug)}
            >
              <i style={{ background: technology.accent }} />{technology.name}
              <span>{technology.lessonCount}</span>
            </button>
          ))}
        </aside> : <aside className="pattern-note"><span>PATTERN HARİTASI</span><p>Bir pattern’i adına göre değil, çözdüğü gerilim ve getirdiği operasyon maliyetiyle değerlendir.</p></aside>}

        <div className="lesson-catalog" aria-live="polite" aria-busy={loading}>
          <div className="catalog-heading">
            <div>
              <span>{selectedTechnology ? technologies.find(x => x.slug === selectedTechnology)?.name : 'Tüm teknolojiler'}</span>
              <h2>{mode === 'lessons' ? 'Ders kataloğu' : 'Pattern kataloğu'}</h2>
            </div>
            <b>{(mode === 'lessons' ? lessons.length : patterns.length).toString().padStart(2, '0')}</b>
          </div>
          {loading ? (
            <div className="guide-state"><span className="loading-line" />Dersler yükleniyor…</div>
          ) : (mode === 'lessons' ? lessons : patterns).length === 0 ? (
            <div className="guide-state"><b>Bu alanda yayınlanmış ders yok.</b><p>Yeni içerikler hazır olduğunda burada görünecek.</p></div>
          ) : (
            <div className="lesson-grid">
              {(mode === 'lessons' ? lessons : patterns).map((item, index) => (
                <article className="lesson-card" key={`${item.stableId}-${item.version}`}>
                  <div className="lesson-index">{String(index + 1).padStart(2, '0')}</div>
                  <div className="lesson-card-body">
                    <div className="lesson-meta">
                      <span>{levelLabels[item.level] ?? item.level}</span>
                      <span>{item.estimatedMinutes} dk</span>
                      {item.technology && <span>{item.technology.name}</span>}
                      {mode === 'patterns' && <span>{(item as PatternSummary).category}</span>}
                    </div>
                    <h3>{item.title}</h3>
                    <p>{item.summary}</p>
                    <button className="lesson-link" onClick={() => mode === 'lessons' ? onLesson(item.slug) : onPattern(item.slug)} disabled={loading}>
                      {mode === 'lessons' ? 'Dersi aç' : 'Pattern’i aç'} <span aria-hidden="true">→</span>
                    </button>
                  </div>
                </article>
              ))}
            </div>
          )}
        </div>
      </div>
    </section>
  )
}

function LessonReader({ lesson, progress, canTrack, onProgress, onBack }: {
  lesson: LessonDetail
  progress: LessonProgress | null
  canTrack: boolean
  onProgress: (lastSectionKey: string, completedSectionKeys: string[]) => Promise<void>
  onBack: () => void
}) {
  const [savingSection, setSavingSection] = useState('')
  const completed = new Set(progress?.completedSectionKeys ?? [])
  const percent = progress ? Math.round((progress.completedSections / progress.totalSections) * 100) : 0

  useEffect(() => {
    if (!progress || progress.completedSections === 0 || progress.completed) return
    const target = document.getElementById(progress.lastSectionKey)
    target?.scrollIntoView?.({ behavior: 'smooth', block: 'start' })
  }, [progress])

  const completeSection = async (section: LessonSection) => {
    const completedKeys = [...new Set([...completed, section.key])]
    const next = lesson.sections.find(item => item.order === section.order + 1)
    setSavingSection(section.key)
    try {
      await onProgress(next?.key ?? section.key, completedKeys)
      if (next) document.getElementById(next.key)?.scrollIntoView?.({ behavior: 'smooth', block: 'start' })
    } finally {
      setSavingSection('')
    }
  }

  return (
    <article className="lesson-reader">
      <button className="text-button reader-back" onClick={onBack}>← Ders kataloğuna dön</button>
      <header className="reader-hero">
        <div className="reader-number">v{lesson.version.toString().padStart(2, '0')}</div>
        <div>
          <div className="lesson-meta">
            <span>{levelLabels[lesson.level] ?? lesson.level}</span>
            <span>{lesson.estimatedMinutes} dk okuma</span>
            {lesson.technology && <span>{lesson.technology.name}</span>}
          </div>
          <h1>{lesson.title}</h1>
          <p>{lesson.summary}</p>
          {canTrack && progress && (
            <div className={`reader-progress ${progress.completed ? 'is-complete' : ''}`}>
              <div>
                <span>{progress.completed ? 'DERS TAMAMLANDI' : 'OKUMA İLERLEMESİ'}</span>
                <b>{progress.completedSections} / {progress.totalSections} bölüm</b>
              </div>
              <div className="reader-progress-track" role="progressbar" aria-label="Ders ilerlemesi" aria-valuemin={0} aria-valuemax={100} aria-valuenow={percent}><i style={{ width: `${percent}%` }} /></div>
            </div>
          )}
        </div>
      </header>

      <div className="reader-layout">
        <aside className="reader-rail">
          <div><small>BU DERSTE</small>{lesson.objectives.map(objective => <p key={objective}>{objective}</p>)}</div>
          {lesson.prerequisites.length > 0 && (
            <div><small>ÖN KOŞULLAR</small>{lesson.prerequisites.map(item => <p key={item}>{item}</p>)}</div>
          )}
          <nav aria-label="Ders bölümleri">
            {lesson.sections.map(section => {
              const isCompleted = completed.has(section.key)
              const isCurrent = progress?.lastSectionKey === section.key && !progress.completed
              return (
                <a
                  key={section.key}
                  href={`#${section.key}`}
                  className={isCompleted ? 'section-complete' : isCurrent ? 'section-current' : ''}
                  aria-current={isCurrent ? 'step' : undefined}
                >
                  <i aria-hidden="true">{isCompleted ? '✓' : section.order}</i>
                  <span>{section.title}</span>
                  {isCurrent && <small>BURADA KALDIN</small>}
                </a>
              )
            })}
          </nav>
          {!canTrack && !('category' in lesson) && <p className="reader-signin-note">İlerlemeni cihazlar arasında korumak için giriş yap.</p>}
        </aside>
        <div className="reader-content">
          {lesson.sections.map(section => (
            <section id={section.key} key={section.key} className="reader-section">
              <span>{section.order.toString().padStart(2, '0')}</span>
              <h2>{section.title}</h2>
              {section.bodyMarkdown.split(/\n\n+/).map((paragraph, index) => <p key={index}>{paragraph}</p>)}
              {section.codeSample && (
                <div className="code-block">
                  <div><span>{section.codeLanguage ?? 'code'}</span><small>ÖRNEK</small></div>
                  <pre tabIndex={0}><code>{section.codeSample}</code></pre>
                </div>
              )}
              {canTrack && progress && (
                <button
                  className="section-complete-button"
                  disabled={completed.has(section.key) || savingSection === section.key}
                  onClick={() => completeSection(section)}
                >
                  {completed.has(section.key)
                    ? '✓ Bölüm tamamlandı'
                    : savingSection === section.key
                      ? 'Kaydediliyor…'
                      : lesson.sections.at(-1)?.key === section.key
                        ? 'Dersi tamamla'
                        : 'Bölümü tamamla →'}
                </button>
              )}
            </section>
          ))}
        </div>
      </div>
    </article>
  )
}

function AuthScreen({ onAuthenticated }: { onAuthenticated: (onboarded: boolean) => void }) {
  const [mode, setMode] = useState<'register' | 'login'>('register')
  const [error, setError] = useState('')
  const [busy, setBusy] = useState(false)

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setBusy(true)
    setError('')
    const form = new FormData(event.currentTarget)
    try {
      const response = await api<{ accessToken: string; onboardingCompleted: boolean }>(`/auth/${mode}`, {
        method: 'POST',
        body: JSON.stringify({
          email: form.get('email'),
          password: form.get('password'),
          displayName: form.get('displayName') || 'Geliştirici',
        }),
      })
      localStorage.setItem('careerforge-token', response.accessToken)
      onAuthenticated(response.onboardingCompleted)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Giriş yapılamadı.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <section className="auth-layout">
      <div className="auth-story">
        <span className="eyebrow">EZBER DEĞİL, KANIT</span>
        <h1>Mülakatta bildiğini değil, <em>nasıl düşündüğünü</em> göster.</h1>
        <p>Teknoloji yığınını ve hedef rolünü seç. CareerForge sana hazır soru listesi değil, ölçülen eksiklerinden başlayan bir çalışma rotası çıkarsın.</p>
        <div className="route-preview" aria-label="Ürün akışı">
          {['Profil', 'Tanılama', 'Çalışma', 'Mülakat', 'Tekrar'].map((item, index) => (
            <div key={item} className={index === 0 ? 'route-current' : ''}>
              <span>{String(index + 1).padStart(2, '0')}</span>{item}
            </div>
          ))}
        </div>
        <div className="proof-row">
          <span><b>4+</b> teknoloji ekosistemi</span>
          <span><b>8</b> ölçüm boyutu</span>
          <span><b>1</b> kişisel rota</span>
        </div>
      </div>
      <div className="auth-panel">
        <div className="auth-tabs" role="tablist" aria-label="Hesap işlemi">
          <button role="tab" aria-selected={mode === 'register'} className={mode === 'register' ? 'active' : ''} onClick={() => setMode('register')}>Hesap oluştur</button>
          <button role="tab" aria-selected={mode === 'login'} className={mode === 'login' ? 'active' : ''} onClick={() => setMode('login')}>Giriş yap</button>
        </div>
        <form onSubmit={submit}>
          <div>
            <span className="form-kicker">{mode === 'register' ? 'Rotanı oluştur' : 'Kaldığın yerden devam et'}</span>
            <h2>{mode === 'register' ? 'İlk tanılamaya hazırlan' : 'Tekrar hoş geldin'}</h2>
          </div>
          {mode === 'register' && <label>Adın<input name="displayName" autoComplete="name" required placeholder="Fatima Saliha" /></label>}
          <label>E-posta<input name="email" type="email" autoComplete="email" required placeholder="sen@ornek.com" /></label>
          <label>Parola<input name="password" type="password" autoComplete={mode === 'register' ? 'new-password' : 'current-password'} required minLength={8} placeholder="En az 8 karakter" /></label>
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="primary wide" disabled={busy}>{busy ? 'Hazırlanıyor…' : mode === 'register' ? 'Yetkinliklerimi seç' : 'Rotamı aç'}</button>
          <p className="privacy-note">Cevaplarına parola veya gerçek proje verisi yazma. Tam cevap metinleri operasyon loglarına alınmaz.</p>
        </form>
      </div>
    </section>
  )
}

function Onboarding({ catalog, onDone, onError }: {
  catalog: { technologies: Technology[]; skills: Skill[]; specializations: Specialization[] }
  onDone: () => void
  onError: (message: string) => void
}) {
  const [step, setStep] = useState(1)
  const [source, setSource] = useState<Source>('specialization')
  const [targetRole, setTargetRole] = useState('Backend Developer')
  const [seniority, setSeniority] = useState('senior')
  const [experience, setExperience] = useState(5)
  const [weeklyMinutes, setWeeklyMinutes] = useState(240)
  const [selectedSpecs, setSelectedSpecs] = useState<string[]>([])
  const [selectedTech, setSelectedTech] = useState<string[]>([])
  const [selectedSkills, setSelectedSkills] = useState<Record<string, Level>>({})
  const [busy, setBusy] = useState(false)

  const chooseSpec = (spec: Specialization) => {
    setSelectedSpecs([spec.id])
    setTargetRole(spec.name)
    setSelectedSkills(Object.fromEntries(spec.skills.map(s => [s.skillId, s.required ? 'intermediate' : 'basic'])))
  }
  const toggleTech = (id: string) =>
    setSelectedTech(current => current.includes(id) ? current.filter(x => x !== id) : [...current, id])
  const toggleSkill = (id: string) =>
    setSelectedSkills(current => current[id] ? Object.fromEntries(Object.entries(current).filter(([key]) => key !== id)) : { ...current, [id]: 'basic' })

  const finish = async () => {
    if (Object.keys(selectedSkills).length === 0) {
      onError('En az bir yetkinlik seçmelisin.')
      return
    }
    setBusy(true)
    try {
      await api('/me/preparation-profile', {
        method: 'PUT',
        body: JSON.stringify({
          source, targetRole, targetSeniority: seniority, experienceYears: experience,
          questionLanguage: 'tr', preferredCodeLanguage: selectedTech[0] ?? 'typescript',
          weeklyStudyMinutes: weeklyMinutes, specializationIds: selectedSpecs,
          technologyIds: selectedTech,
          skills: Object.entries(selectedSkills).map(([skillId, selfAssessedLevel]) => ({
            skillId, selfAssessedLevel, targetLevel: seniority === 'junior' ? 'intermediate' : 'advanced',
          })),
        }),
      })
      onDone()
    } catch (error) {
      onError(error instanceof Error ? error.message : 'Profil kaydedilemedi.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <section className="onboarding">
      <div className="stepper" role="list" aria-label="Profil adımları">
        {['Hedef', 'Deneyim', 'Alanlar', 'Kontrol'].map((label, index) => (
          <div role="listitem" aria-current={step === index + 1 ? 'step' : undefined} className={step >= index + 1 ? 'step-active' : ''} key={label}>
            <span>{index + 1}</span><b>{label}</b>
          </div>
        ))}
      </div>

      {step === 1 && (
        <div className="onboard-card">
          <span className="eyebrow">1 / 4 · BAŞLANGIÇ NOKTASI</span>
          <h1>Nasıl hazırlanmak istiyorsun?</h1>
          <p className="lead">Bu seçim yalnızca ilk rotanı belirler. İlerleme geçmişin korunarak daha sonra değiştirebilirsin.</p>
          <div className="choice-grid">
            {sources.map(item => (
              <button aria-pressed={source === item.id} key={item.id} className={`choice-card ${source === item.id ? 'selected' : ''}`} onClick={() => setSource(item.id)}>
                <span className="choice-radio" />
                <strong>{item.label}</strong>
                <small>{item.copy}</small>
              </button>
            ))}
          </div>
        </div>
      )}

      {step === 2 && (
        <div className="onboard-card">
          <span className="eyebrow">2 / 4 · HEDEFİNİN BAĞLAMI</span>
          <h1>Seviyeyi yıl değil, kanıt belirlesin.</h1>
          <div className="form-grid">
            <label>Hedef rol<input value={targetRole} onChange={e => setTargetRole(e.target.value)} /></label>
            <label>Hedef seviye<select value={seniority} onChange={e => setSeniority(e.target.value)}>
              <option value="junior">Junior</option><option value="mid">Mid</option><option value="senior">Senior</option><option value="lead">Lead</option>
            </select></label>
            <label>Toplam deneyim<input type="number" min="0" max="60" value={experience} onChange={e => setExperience(Number(e.target.value))} /><span className="input-suffix">yıl</span></label>
            <label>Haftalık çalışma<input type="number" min="30" step="30" value={weeklyMinutes} onChange={e => setWeeklyMinutes(Number(e.target.value))} /><span className="input-suffix">dk</span></label>
          </div>
          <div className="info-strip"><b>Not:</b> Deneyim yılı yalnızca soru dağılımını başlatır; ölçülen seviye tanılama ve sonraki oturumlarla oluşur.</div>
        </div>
      )}

      {step === 3 && (
        <div className="onboard-card wide-card">
          <span className="eyebrow">3 / 4 · YETKİNLİK HARİTASI</span>
          <h1>{source === 'specialization' ? 'Bir uzmanlık rotası seç.' : 'Çalışmak istediğin alanları işaretle.'}</h1>
          {source === 'specialization' && (
            <div className="specialization-row">
              {catalog.specializations.map(spec => (
                <button aria-pressed={selectedSpecs.includes(spec.id)} key={spec.id} className={`spec-card ${selectedSpecs.includes(spec.id) ? 'selected' : ''}`} onClick={() => chooseSpec(spec)}>
                  <strong>{spec.name}</strong><span>{spec.description}</span><small>{spec.skills.length} yetkinlik</small>
                </button>
              ))}
            </div>
          )}
          <div className="selection-columns">
            <div>
              <h3>Teknoloji ekosistemi <span>{selectedTech.length}</span></h3>
              <div className="tag-grid">
                {catalog.technologies.map(tech => (
                  <button aria-pressed={selectedTech.includes(tech.id)} key={tech.id} onClick={() => toggleTech(tech.id)} className={selectedTech.includes(tech.id) ? 'tag selected' : 'tag'}>
                    <i style={{ background: tech.accent }} />{tech.name}<small>{tech.maturity}</small>
                  </button>
                ))}
              </div>
            </div>
            <div>
              <h3>Teknik yetkinlik <span>{Object.keys(selectedSkills).length}</span></h3>
              <div className="skill-list">
                {catalog.skills.map(skill => (
                  <div className={`skill-option ${selectedSkills[skill.id] ? 'selected' : ''}`} key={skill.id}>
                    <button aria-pressed={Boolean(selectedSkills[skill.id])} onClick={() => toggleSkill(skill.id)}><span aria-hidden="true" className="check-box">{selectedSkills[skill.id] ? '✓' : ''}</span><b>{skill.name}</b><small>{skill.description}</small></button>
                    {selectedSkills[skill.id] && (
                      <select aria-label={`${skill.name} öz değerlendirme`} value={selectedSkills[skill.id]} onChange={e => setSelectedSkills({ ...selectedSkills, [skill.id]: e.target.value as Level })}>
                        {Object.entries(levelLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
                      </select>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      )}

      {step === 4 && (
        <div className="onboard-card">
          <span className="eyebrow">4 / 4 · İLK ROTA</span>
          <h1>Tanılama için başlangıç profilin hazır.</h1>
          <div className="review-grid">
            <div><small>HEDEF</small><b>{targetRole}</b><span>{seniority} · {experience} yıl deneyim</span></div>
            <div><small>ÇALIŞMA RİTMİ</small><b>Haftada {weeklyMinutes} dakika</b><span>Plan ilerlemene göre güncellenecek</span></div>
            <div><small>TEKNOLOJİ</small><b>{selectedTech.length} seçim</b><span>{catalog.technologies.filter(x => selectedTech.includes(x.id)).map(x => x.name).join(', ') || 'Teknolojiden bağımsız'}</span></div>
            <div><small>YETKİNLİK</small><b>{Object.keys(selectedSkills).length} alan</b><span>Öz beyan, tanılama sonucundan ayrı tutulacak</span></div>
          </div>
          <div className="confidence-note"><span>?</span><p><b>Neden hemen bir seviye vermiyoruz?</b> Güvenilir bir seviye için farklı soru türlerinden yeterli kanıt gerekir. İlk tanılama sana seviyenin yanında ölçüm güvenini de gösterecek.</p></div>
        </div>
      )}

      <div className="onboard-actions">
        <button className="secondary" disabled={step === 1} onClick={() => setStep(step - 1)}>Geri</button>
        {step < 4
          ? <button className="primary" onClick={() => setStep(step + 1)}>Devam et <span>→</span></button>
          : <button className="primary" disabled={busy} onClick={finish}>{busy ? 'Rota hazırlanıyor…' : 'Rotamı oluştur →'}</button>}
      </div>
    </section>
  )
}

function Dashboard({ skills, path, summary, technologies, onDiagnostic, onInterview, onReview, onLearning, onEdit, loading }: {
  skills: UserSkill[]
  path: PathItem[]
  summary: DashboardSummary | null
  technologies: Technology[]
  onDiagnostic: () => void
  onInterview: () => void
  onReview: () => void
  onLearning: () => void
  onEdit: () => void
  loading: boolean
}) {
  const measured = skills.filter(x => x.measuredLevel)
  const averageConfidence = measured.length ? Math.round(measured.reduce((sum, x) => sum + Number(x.confidenceScore ?? 0), 0) / measured.length) : 0
  const nextAction = summary?.nextWork.kind === 'review'
    ? onReview
    : summary?.nextWork.kind === 'path'
      ? onLearning
      : onDiagnostic
  const nextActionLabel = summary?.nextWork.kind === 'review'
    ? 'Tekrarları aç'
    : summary?.nextWork.kind === 'path'
      ? 'Rehberi aç'
      : 'Tanılamayı başlat'
  return (
    <section className="dashboard">
      <div className="dash-intro">
        <div><span className="eyebrow">KİŞİSEL ÇALIŞMA ROTASI</span><h1>Bugün ezber değil, <em>bir karar</em> çalış.</h1><p>Rotan öz beyanından başlar; cevap verdikçe ölçülen seviyenle yeniden şekillenir.</p></div>
        <div className="week-card">
          <small>SIRADAKİ ÇALIŞMA</small>
          <b>{summary?.nextWork.title ?? path[0]?.title ?? 'İlk tanılamanı tamamla'}</b>
          <span>{summary?.nextWork.description ?? path[0]?.reason ?? 'Yetkinlik haritan için ilk kanıtları topla.'}</span>
          <button className="text-button" onClick={nextAction}>{nextActionLabel} →</button>
        </div>
      </div>
      <div className="evidence-strip" aria-label="Çalışma özeti">
        <article className={summary?.dueReviewCount ? 'is-urgent' : ''}>
          <small>BUGÜN</small>
          <b>{summary?.dueReviewCount ?? 0}</b>
          <span>{summary?.dueReviewCount === 1 ? 'soru tekrar bekliyor' : 'soru tekrar bekliyor'}</span>
        </article>
        <article>
          <small>ZAYIF ALAN</small>
          <b>{summary?.weakestSkill?.skill ?? 'Henüz ölçülmedi'}</b>
          <span>
            {summary?.weakestSkill
              ? `${summary.weakestSkill.measuredLevel ? levelLabels[summary.weakestSkill.measuredLevel] : 'İlk kanıt bekleniyor'} · %${Math.round(summary.weakestSkill.confidenceScore)} güven`
              : 'Tanılama sonrası burada görünür'}
          </span>
        </article>
        <article>
          <small>SON KANIT</small>
          <b>{summary?.lastResult ? `${summary.lastResult.score} / 100` : 'Oturum yok'}</b>
          <span>
            {summary?.lastResult
              ? `${summary.lastResult.answeredQuestions} cevap · ${summary.lastResult.kind === 'diagnostic' ? 'Tanılama' : 'Mülakat'}`
              : 'İlk oturumunu tamamla'}
          </span>
        </article>
      </div>
      <div className="dash-grid">
        <div className="main-column">
          <article className="panel route-map">
            <div className="panel-title"><div><span>01</span><h2>Yetkinlik rotan</h2></div><button className="text-button" onClick={onEdit}>Profili düzenle</button></div>
            {skills.length === 0 ? <Empty title="Henüz yetkinlik seçilmedi" copy="Profilini tamamlayarak kişisel rotanı oluştur." action="Profili tamamla" onAction={onEdit} /> : (
              <div className="skill-map">
                {skills.map((skill, index) => {
                  const self = skill.selfAssessedLevel ? Object.keys(levelLabels).indexOf(skill.selfAssessedLevel.toLowerCase()) + 1 : 0
                  const measuredLevel = skill.measuredLevel ? Object.keys(levelLabels).indexOf(skill.measuredLevel.toLowerCase()) + 1 : 0
                  return (
                    <div className="skill-row" key={skill.id ?? `${skill.skillId}-${index}`}>
                      <div className="skill-name"><b>{skill.skill}</b><span>{skill.technology ?? 'Ortak mühendislik'}</span></div>
                      <div className="level-track">
                        <div className="level-axis">{[1, 2, 3, 4, 5].map(point => <i className={point <= (measuredLevel || self) ? 'filled' : ''} key={point} />)}</div>
                        <div className="level-caption">
                          <span>Öz beyan: {levelLabels[skill.selfAssessedLevel?.toLowerCase() ?? ''] ?? '—'}</span>
                          <b>{skill.measuredLevel ? `Ölçülen: ${levelLabels[skill.measuredLevel.toLowerCase()]}` : 'Henüz ölçülmedi'}</b>
                        </div>
                      </div>
                      <div className="confidence"><b>%{Math.round(Number(skill.confidenceScore ?? 0))}</b><span>kanıt güveni</span></div>
                    </div>
                  )
                })}
              </div>
            )}
          </article>

          <article className="panel">
            <div className="panel-title"><div><span>02</span><h2>Sıradaki çalışma</h2></div><small>{path.length} adım</small></div>
            <div className="path-list">
              {path.slice(0, 4).map((item, index) => (
                <div key={item.id} className="path-row">
                  <span className="path-number">{String(index + 1).padStart(2, '0')}</span>
                  <div><b>{item.title}</b><p>{item.reason}</p></div>
                  <span className="path-status">{index === 0 ? 'Şimdi' : 'Sırada'}</span>
                </div>
              ))}
              {path.length === 0 && <Empty title="Rota tanılama bekliyor" copy="İlk oturumdan sonra eksiklerin için ders sırası burada görünecek." />}
            </div>
          </article>
        </div>

        <aside className="side-column">
          <article className="action-card diagnostic-card">
            <small>8 SORU · 15 DAKİKA</small><h2>Başlangıç tanılaması</h2><p>Farklı soru türlerinden ilk kanıtlarını topla. Öz beyanın sonuçtan ayrı kalır.</p>
            <button className="primary wide" onClick={onDiagnostic} disabled={loading}>Yeni tanılama başlat →</button>
          </article>
          <article className="action-card interview-card">
            <small>10 SORU · SERBEST MOD</small><h2>Mülakat provası</h2><p>Seçtiğin teknoloji ve yetkinliklerle dengeli bir oturum oluştur.</p>
            <button className="secondary wide" onClick={onInterview} disabled={loading}>Prova oluştur</button>
          </article>
          <article className="panel metric-card">
            <div><span>Ölçülen alan</span><b>{measured.length}<small> / {skills.length}</small></b></div>
            <div><span>Ortalama güven</span><b>%{averageConfidence}</b></div>
            <div><span>Teknoloji kataloğu</span><b>{technologies.length}</b></div>
          </article>
        </aside>
      </div>
    </section>
  )
}

function SessionScreen({ session, onCancel, onComplete, onError }: {
  session: SessionData
  onCancel: () => void
  onComplete: (result: SessionData) => void
  onError: (message: string) => void
}) {
  const [index, setIndex] = useState(0)
  const [answer, setAnswer] = useState('')
  const [score, setScore] = useState(50)
  const [answers, setAnswers] = useState<Record<string, { answer: string; score: number }>>({})
  const [busy, setBusy] = useState(false)
  const current = session.questions[index]
  const base = session.kind === 'diagnostic' ? 'diagnostic-sessions' : 'interview-sessions'
  const progress = Math.round(((index + 1) / session.questions.length) * 100)

  const saveAndNext = async () => {
    if (answer.trim().length < 10) {
      onError('Cevabın en az 10 karakter olmalı; düşünce sürecini kısaca açıkla.')
      return
    }
    setBusy(true)
    try {
      await api(`/${base}/${session.id}/answers/${current.id}`, { method: 'POST', body: JSON.stringify({ answerText: answer, selfScore: score }) })
      setAnswers({ ...answers, [current.id]: { answer, score } })
      if (index < session.questions.length - 1) {
        const next = answers[session.questions[index + 1].id]
        setIndex(index + 1)
        setAnswer(next?.answer ?? '')
        setScore(next?.score ?? 50)
      } else {
        await api(`/${base}/${session.id}/complete`, { method: 'POST' })
        const completed = await api<SessionData>(`/${base}/${session.id}/result`)
        onComplete(completed)
      }
    } catch (error) {
      onError(error instanceof Error ? error.message : 'Cevap kaydedilemedi.')
    } finally {
      setBusy(false)
    }
  }

  return (
    <section className="session-page">
      <div className="session-head">
        <button className="text-button" onClick={onCancel}>← Rotaya dön</button>
        <div><span>{session.kind === 'diagnostic' ? 'Tanılama' : 'Mülakat provası'}</span><b>{index + 1} / {session.questions.length}</b></div>
      </div>
      <div className="progress-line" role="progressbar" aria-label="Oturum ilerlemesi" aria-valuemin={0} aria-valuemax={100} aria-valuenow={progress}><i style={{ width: `${progress}%` }} /></div>
      <div className="session-layout">
        <article className="question-panel">
          <div className="question-meta"><span>{current.type}</span><span>{levelLabels[current.level] ?? current.level}</span><span>{current.technology ?? 'Teknolojiden bağımsız'}</span></div>
          <small className="question-skill">{current.skill}</small>
          <h1>{current.prompt}</h1>
          <label className="answer-field">Cevabın<textarea value={answer} onChange={e => setAnswer(e.target.value)} rows={10} placeholder="Önce teşhisini, sonra kararını, trade-off'u ve nasıl doğrulayacağını anlat…" /></label>
          <div className="score-field">
            <div><b>Kendi cevabını nasıl değerlendirirsin?</b><span>Bu öz puan, sistem ölçümünden ayrı tutulur.</span></div>
            <output>{score}</output>
            <input type="range" min="0" max="100" step="5" value={score} onChange={e => setScore(Number(e.target.value))} aria-label="Öz değerlendirme puanı" />
          </div>
          <button className="primary" disabled={busy} onClick={saveAndNext}>{busy ? 'Kaydediliyor…' : index === session.questions.length - 1 ? 'Oturumu tamamla' : 'Kaydet ve sonraki →'}</button>
        </article>
        <aside className="session-aside">
          <span className="eyebrow">CEVAP İSKELETİ</span>
          <ol><li>Problemi ve varsayımları netleştir.</li><li>İlk toplayacağın kanıtı söyle.</li><li>Kararını ve alternatifini açıkla.</li><li>Başarıyı nasıl ölçeceğini belirt.</li></ol>
          <div className="privacy-box"><b>Gerçek proje verisini maskele.</b><p>Kurum, müşteri, erişim bilgisi veya kişisel veri kullanma.</p></div>
        </aside>
      </div>
    </section>
  )
}

function ResultScreen({ result, onAddReview, onDone }: {
  result: SessionData
  onAddReview: (questionId: string) => Promise<void>
  onDone: () => void
}) {
  const answered = result.questions.filter(x => x.answered)
  const [savedQuestions, setSavedQuestions] = useState<string[]>([])
  const [savingQuestion, setSavingQuestion] = useState('')

  const saveForReview = async (questionId: string) => {
    setSavingQuestion(questionId)
    try {
      await onAddReview(questionId)
      setSavedQuestions(current => [...current, questionId])
    } finally {
      setSavingQuestion('')
    }
  }

  return (
    <section className="result-page">
      <div className="result-hero"><span className="eyebrow">OTURUM TAMAMLANDI</span><h1>Tek puan değil, <em>kanıt haritası.</em></h1><p>{answered.length} cevap kaydedildi. Her alanın güveni, yeterli ve farklı kanıtlar geldikçe yükselecek.</p></div>
      <div className="result-list">
        {result.questions.map((question, index) => (
          <details key={question.id} className="result-question" open={index === 0}>
            <summary><span>{String(index + 1).padStart(2, '0')}</span><div><b>{question.skill}</b><p>{question.prompt}</p></div><i>+</i></summary>
            <div className="result-answer">
              {question.evaluation && (
                <section className="rubric-result" aria-label="Rubric değerlendirmesi">
                  <div className="rubric-score">
                    <div><small>SİSTEM ÖLÇÜMÜ</small><b>{question.evaluation.overallScore}</b><span>/ 100</span></div>
                    <p>{question.evaluation.rubric} · v{question.evaluation.rubricVersion}<br />Öz değerlendirmen: {question.selfScore ?? '—'}</p>
                  </div>
                  <div className="rubric-dimensions">
                    {question.evaluation.dimensions.map(dimension => (
                      <article key={dimension.key}>
                        <header><b>{dimension.label}</b><span>{dimension.score} / 100 · %{dimension.weight}</span></header>
                        <div className="rubric-meter" role="progressbar" aria-label={`${dimension.label} puanı`} aria-valuemin={0} aria-valuemax={100} aria-valuenow={dimension.score}><i style={{ width: `${dimension.score}%` }} /></div>
                        <p>{dimension.feedback}</p>
                      </article>
                    ))}
                  </div>
                </section>
              )}
              <div><small>MODEL YAKLAŞIM</small><p>{question.modelAnswer}</p></div>
              <div className="signal-columns"><section><small>GÜÇLÜ SİNYALLER</small><ul>{question.signals?.map(signal => <li key={signal}>{signal}</li>)}</ul></section><section><small>RİSKLİ YAKLAŞIMLAR</small><ul>{question.redFlags?.map(flag => <li key={flag}>{flag}</li>)}</ul></section></div>
              <button
                className="review-save"
                disabled={savedQuestions.includes(question.id) || savingQuestion === question.id}
                onClick={() => saveForReview(question.id)}
              >
                {savedQuestions.includes(question.id) ? '✓ Tekrar listesinde' : savingQuestion === question.id ? 'Ekleniyor…' : '+ Tekrar listesine ekle'}
              </button>
            </div>
          </details>
        ))}
      </div>
      <button className="primary" onClick={onDone}>Güncellenen rotamı gör →</button>
    </section>
  )
}

function ReviewScreen({ items, loading, onRemove, onReview, onPractice }: {
  items: ReviewItem[]
  loading: boolean
  onRemove: (questionId: string) => Promise<void>
  onReview: (questionId: string, rating: string) => Promise<void>
  onPractice: () => void
}) {
  const [skill, setSkill] = useState('')
  const [level, setLevel] = useState('')
  const [schedule, setSchedule] = useState('')
  const [removing, setRemoving] = useState('')
  const [reviewing, setReviewing] = useState('')
  const skills = [...new Map(items.map(item => [item.skillSlug, item.skill])).entries()]
  const now = Date.now()
  const visible = items.filter(item =>
    (!skill || item.skillSlug === skill)
    && (!level || item.level === level)
    && (!schedule || (schedule === 'due' ? new Date(item.nextReviewAt).getTime() <= now : new Date(item.nextReviewAt).getTime() > now)))
  const dueCount = items.filter(item => new Date(item.nextReviewAt).getTime() <= now).length

  const remove = async (questionId: string) => {
    setRemoving(questionId)
    try {
      await onRemove(questionId)
    } finally {
      setRemoving('')
    }
  }

  const review = async (questionId: string, rating: string) => {
    setReviewing(`${questionId}:${rating}`)
    try {
      await onReview(questionId, rating)
    } finally {
      setReviewing('')
    }
  }

  return (
    <section className="review-page">
      <header className="review-hero">
        <div><span className="eyebrow">GERİ DÖNÜŞ KUYRUĞU</span><h1>Bir kez cevapladın.<br /><em>Şimdi sağlamlaştır.</em></h1></div>
        <p>Bugün sıran gelen {dueCount} soru var. Hatırlama kaliteni işaretle; çalışma takvimi bir sonraki aralığı kendisi kursun.</p>
      </header>
      <div className="review-layout">
        <aside className="review-filters">
          <div><small>FİLTRELE</small><b>{visible.length} / {items.length}</b></div>
          <label>Beceri
            <select value={skill} onChange={event => setSkill(event.target.value)}>
              <option value="">Tüm beceriler</option>
              {skills.map(([slug, name]) => <option key={slug} value={slug}>{name}</option>)}
            </select>
          </label>
          <label>Seviye
            <select value={level} onChange={event => setLevel(event.target.value)}>
              <option value="">Tüm seviyeler</option>
              {Object.entries(levelLabels).map(([value, label]) => <option key={value} value={value}>{label}</option>)}
            </select>
          </label>
          <label>Zaman
            <select value={schedule} onChange={event => setSchedule(event.target.value)}>
              <option value="">Tüm sorular</option>
              <option value="due">Bugün çalış</option>
              <option value="upcoming">Planlandı</option>
            </select>
          </label>
        </aside>
        <div className="review-ledger">
          <div className="review-ledger-head"><span>TEKRAR DEFTERİ</span><b>{visible.length.toString().padStart(2, '0')}</b></div>
          {loading ? (
            <div className="guide-state"><div className="loading-line" /><p>Tekrar listesi yükleniyor…</p></div>
          ) : visible.length === 0 ? (
            <Empty
              title={items.length ? 'Bu filtrede soru yok' : 'Tekrar listen boş'}
              copy={items.length ? 'Başka bir beceri veya seviye seç.' : 'Oturum sonucunda tekrar etmek istediğin soruları listeye ekle.'}
              action={items.length ? undefined : 'Mülakat provası başlat'}
              onAction={items.length ? undefined : onPractice}
            />
          ) : visible.map((item, index) => (
            <article className="review-row" key={item.id}>
              <span>{String(index + 1).padStart(2, '0')}</span>
              <div>
                <div className="question-meta"><span>{item.skill}</span><span>{levelLabels[item.level] ?? item.level}</span>{item.technology && <span>{item.technology}</span>}</div>
                <h2>{item.prompt}</h2>
                <small>{item.type} · {item.repetitionCount ? `${item.repetitionCount} tekrar` : 'Henüz çalışılmadı'}</small>
                <div className="review-rating" role="group" aria-label="Hatırlama kalitesi">
                  {[
                    ['again', 'Tekrar', '1 gün'],
                    ['hard', 'Zor', 'kısa'],
                    ['good', 'İyi', 'normal'],
                    ['easy', 'Kolay', 'uzun'],
                  ].map(([value, label, hint]) => (
                    <button
                      key={value}
                      aria-label={`${label}, ${hint}`}
                      disabled={Boolean(reviewing)}
                      onClick={() => review(item.questionId, value)}
                    >
                      <b>{reviewing === `${item.questionId}:${value}` ? '…' : label}</b>
                      <span>{hint}</span>
                    </button>
                  ))}
                </div>
              </div>
              <div className={`review-date ${new Date(item.nextReviewAt).getTime() <= now ? 'is-due' : ''}`}>
                <small>SONRAKİ</small>
                <b>{new Date(item.nextReviewAt).getTime() <= now ? 'Bugün' : new Date(item.nextReviewAt).toLocaleDateString('tr-TR', { day: '2-digit', month: 'short' })}</b>
                <span>{item.intervalDays ? `${item.intervalDays} gün aralık` : 'ilk tekrar'}</span>
                <button disabled={removing === item.questionId} onClick={() => remove(item.questionId)}>
                  {removing === item.questionId ? 'Çıkarılıyor…' : 'Listeden çıkar'}
                </button>
              </div>
            </article>
          ))}
        </div>
      </div>
    </section>
  )
}

function Empty({ title, copy, action, onAction }: { title: string; copy: string; action?: string; onAction?: () => void }) {
  return <div className="empty"><span>···</span><b>{title}</b><p>{copy}</p>{action && <button className="text-button" onClick={onAction}>{action} →</button>}</div>
}

export default App
