import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api'

type Level = 'beginner' | 'basic' | 'intermediate' | 'advanced' | 'expert'
type Source = 'skills' | 'specialization' | 'jobRequirements' | 'general'
type Screen = 'auth' | 'onboarding' | 'dashboard' | 'session' | 'result'
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
}
type SessionData = { id: string; kind: string; status: string; questions: SessionQuestion[] }
type PathItem = { id: string; title: string; reason: string; order: number; completed: boolean }

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
    throw new Error(body?.detail ?? body?.title ?? 'İşlem tamamlanamadı.')
  }
  if (response.status === 204) return undefined as T
  return response.json()
}

function App() {
  const [screen, setScreen] = useState<Screen>(localStorage.getItem('careerforge-token') ? 'dashboard' : 'auth')
  const [catalog, setCatalog] = useState<{ technologies: Technology[]; skills: Skill[]; specializations: Specialization[] }>({
    technologies: [], skills: [], specializations: [],
  })
  const [userSkills, setUserSkills] = useState<UserSkill[]>([])
  const [path, setPath] = useState<PathItem[]>([])
  const [session, setSession] = useState<SessionData | null>(null)
  const [result, setResult] = useState<SessionData | null>(null)
  const [message, setMessage] = useState('')
  const [loading, setLoading] = useState(false)

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
      const [skills, learningPath] = await Promise.all([
        api<UserSkill[]>('/me/skills'),
        api<{ items: PathItem[] }>('/learning-paths/current').catch(() => ({ items: [] })),
      ])
      setUserSkills(skills)
      setPath(learningPath.items)
    } catch {
      localStorage.removeItem('careerforge-token')
      setScreen('auth')
    }
  }

  useEffect(() => {
    loadCatalog().catch(() => setMessage('Katalog yüklenemedi. API servisinin çalıştığını kontrol edin.'))
    if (screen === 'dashboard') loadDashboard()
  }, [])

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

  return (
    <div className="app-shell">
      <header className="topbar">
        <button className="brand" onClick={() => screen !== 'auth' && setScreen('dashboard')} aria-label="CareerForge ana sayfa">
          <span className="brand-mark">CF</span>
          <span><strong>CareerForge</strong><small>Mülakat çalışma sistemi</small></span>
        </button>
        {screen !== 'auth' && (
          <nav aria-label="Ana menü">
            <button className={screen === 'dashboard' ? 'nav-active' : ''} onClick={() => { setScreen('dashboard'); loadDashboard() }}>Rotam</button>
            <button onClick={() => startSession('interview')}>Mülakat</button>
            <button onClick={logout}>Çıkış</button>
          </nav>
        )}
      </header>

      {message && <div className="notice" role="status">{message}<button onClick={() => setMessage('')}>Kapat</button></div>}

      <main>
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
            technologies={catalog.technologies}
            onDiagnostic={() => startSession('diagnostic')}
            onInterview={() => startSession('interview')}
            onEdit={() => setScreen('onboarding')}
            loading={loading}
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
          <ResultScreen result={result} onDone={() => { setScreen('dashboard'); loadDashboard() }} />
        )}
      </main>
    </div>
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
        <div className="auth-tabs" role="tablist">
          <button className={mode === 'register' ? 'active' : ''} onClick={() => setMode('register')}>Hesap oluştur</button>
          <button className={mode === 'login' ? 'active' : ''} onClick={() => setMode('login')}>Giriş yap</button>
        </div>
        <form onSubmit={submit}>
          <div>
            <span className="form-kicker">{mode === 'register' ? 'Rotanı oluştur' : 'Kaldığın yerden devam et'}</span>
            <h2>{mode === 'register' ? 'İlk tanılamaya hazırlan' : 'Tekrar hoş geldin'}</h2>
          </div>
          {mode === 'register' && <label>Adın<input name="displayName" autoComplete="name" required placeholder="Fatima Saliha" /></label>}
          <label>E-posta<input name="email" type="email" autoComplete="email" required placeholder="sen@ornek.com" /></label>
          <label>Parola<input name="password" type="password" autoComplete={mode === 'register' ? 'new-password' : 'current-password'} required minLength={8} placeholder="En az 8 karakter" /></label>
          {error && <p className="form-error">{error}</p>}
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
      <div className="stepper">
        {['Hedef', 'Deneyim', 'Alanlar', 'Kontrol'].map((label, index) => (
          <div className={step >= index + 1 ? 'step-active' : ''} key={label}>
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
              <button key={item.id} className={`choice-card ${source === item.id ? 'selected' : ''}`} onClick={() => setSource(item.id)}>
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
                <button key={spec.id} className={`spec-card ${selectedSpecs.includes(spec.id) ? 'selected' : ''}`} onClick={() => chooseSpec(spec)}>
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
                  <button key={tech.id} onClick={() => toggleTech(tech.id)} className={selectedTech.includes(tech.id) ? 'tag selected' : 'tag'}>
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
                    <button onClick={() => toggleSkill(skill.id)}><span className="check-box">{selectedSkills[skill.id] ? '✓' : ''}</span><b>{skill.name}</b><small>{skill.description}</small></button>
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

function Dashboard({ skills, path, technologies, onDiagnostic, onInterview, onEdit, loading }: {
  skills: UserSkill[]
  path: PathItem[]
  technologies: Technology[]
  onDiagnostic: () => void
  onInterview: () => void
  onEdit: () => void
  loading: boolean
}) {
  const measured = skills.filter(x => x.measuredLevel)
  const averageConfidence = measured.length ? Math.round(measured.reduce((sum, x) => sum + Number(x.confidenceScore ?? 0), 0) / measured.length) : 0
  return (
    <section className="dashboard">
      <div className="dash-intro">
        <div><span className="eyebrow">KİŞİSEL ÇALIŞMA ROTASI</span><h1>Bugün ezber değil, <em>bir karar</em> çalış.</h1><p>Rotan öz beyanından başlar; cevap verdikçe ölçülen seviyenle yeniden şekillenir.</p></div>
        <div className="week-card"><small>BU HAFTAKİ ODAK</small><b>{path[0]?.title ?? 'İlk tanılamanı tamamla'}</b><span>{path[0]?.reason ?? 'Yetkinlik haritan için ilk kanıtları topla.'}</span></div>
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
            <button className="primary wide" onClick={onDiagnostic} disabled={loading}>Tanılamayı başlat →</button>
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
      <div className="progress-line"><i style={{ width: `${progress}%` }} /></div>
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

function ResultScreen({ result, onDone }: { result: SessionData; onDone: () => void }) {
  const answered = result.questions.filter(x => x.answered)
  return (
    <section className="result-page">
      <div className="result-hero"><span className="eyebrow">OTURUM TAMAMLANDI</span><h1>Tek puan değil, <em>kanıt haritası.</em></h1><p>{answered.length} cevap kaydedildi. Her alanın güveni, yeterli ve farklı kanıtlar geldikçe yükselecek.</p></div>
      <div className="result-list">
        {result.questions.map((question, index) => (
          <details key={question.id} className="result-question" open={index === 0}>
            <summary><span>{String(index + 1).padStart(2, '0')}</span><div><b>{question.skill}</b><p>{question.prompt}</p></div><i>+</i></summary>
            <div className="result-answer">
              <div><small>MODEL YAKLAŞIM</small><p>{question.modelAnswer}</p></div>
              <div className="signal-columns"><section><small>GÜÇLÜ SİNYALLER</small><ul>{question.signals?.map(signal => <li key={signal}>{signal}</li>)}</ul></section><section><small>RİSKLİ YAKLAŞIMLAR</small><ul>{question.redFlags?.map(flag => <li key={flag}>{flag}</li>)}</ul></section></div>
            </div>
          </details>
        ))}
      </div>
      <button className="primary" onClick={onDone}>Güncellenen rotamı gör →</button>
    </section>
  )
}

function Empty({ title, copy, action, onAction }: { title: string; copy: string; action?: string; onAction?: () => void }) {
  return <div className="empty"><span>···</span><b>{title}</b><p>{copy}</p>{action && <button className="text-button" onClick={onAction}>{action} →</button>}</div>
}

export default App
