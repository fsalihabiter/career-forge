import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'

type ContentKind = 'lessons' | 'patterns' | 'rubrics' | 'questions'
type ContentItem = { stableId: string; version: number; title?: string; prompt?: string; status: string; slug?: string }

const kinds: { id: ContentKind; label: string; singular: string; code: string }[] = [
  { id: 'lessons', label: 'Dersler', singular: 'ders', code: 'LS' },
  { id: 'patterns', label: 'Patternler', singular: 'pattern', code: 'PT' },
  { id: 'rubrics', label: 'Rubricler', singular: 'rubric', code: 'RB' },
  { id: 'questions', label: 'Sorular', singular: 'soru', code: 'QS' },
]

const templates: Record<ContentKind, object> = {
  lessons: { stableId: 'new-lesson', version: 1, slug: 'new-lesson', title: 'Yeni ders', summary: '', technologySlug: null, level: 'intermediate', estimatedMinutes: 20, status: 'draft', objectives: [], prerequisites: [], category: null, sections: [{ key: 'introduction', title: 'Giriş', order: 1, bodyMarkdown: '', codeLanguage: null, codeSample: null }] },
  patterns: { stableId: 'new-pattern', version: 1, slug: 'new-pattern', title: 'Yeni pattern', summary: '', technologySlug: null, level: 'intermediate', estimatedMinutes: 20, status: 'draft', objectives: [], prerequisites: [], category: 'Mimari', sections: [{ key: 'context', title: 'Bağlam', order: 1, bodyMarkdown: '', codeLanguage: null, codeSample: null }] },
  rubrics: { stableId: 'new-rubric', version: 1, title: 'Yeni rubric', description: '', status: 'draft', dimensions: [{ key: 'evidence', label: 'Kanıt', description: '', weight: 100, order: 1 }] },
  questions: { stableId: 'new-question', version: 1, prompt: 'Yeni soru', type: 'open-ended', level: 'intermediate', skillSlug: 'api-design', technologySlug: null, rubricStableId: 'default-technical-answer', rubricVersion: 1, modelAnswer: '', expectedSignals: [''], redFlags: [''], status: 'draft' },
}

export function AdminContent({ request, onMessage, canPublish }: {
  request: <T>(path: string, options?: RequestInit) => Promise<T>
  onMessage: (message: string) => void
  canPublish: boolean
}) {
  const [kind, setKind] = useState<ContentKind>('lessons')
  const [items, setItems] = useState<ContentItem[]>([])
  const [selected, setSelected] = useState<ContentItem | null>(null)
  const [draft, setDraft] = useState('')
  const [isNew, setIsNew] = useState(false)
  const [filter, setFilter] = useState('')
  const [busy, setBusy] = useState(false)
  const meta = kinds.find(x => x.id === kind)!

  const load = useCallback(async (nextKind = kind) => {
    setBusy(true)
    try {
      setItems(await request<ContentItem[]>(`/admin/content/${nextKind}/`))
      setSelected(null); setDraft(''); setIsNew(false)
    } catch (error) {
      onMessage(error instanceof Error ? error.message : 'İçerikler yüklenemedi.')
    } finally { setBusy(false) }
  }, [kind, onMessage, request])

  useEffect(() => { load(kind) }, [kind, load])

  const visibleItems = useMemo(() => {
    const query = filter.trim().toLocaleLowerCase('tr')
    return query ? items.filter(item => [item.title, item.prompt, item.stableId, item.slug]
      .some(value => value?.toLocaleLowerCase('tr').includes(query))) : items
  }, [filter, items])

  const selectItem = async (item: ContentItem) => {
    setBusy(true)
    try {
      const detail = await request<ContentItem>(`/admin/content/${kind}/${item.stableId}/${item.version}`)
      setSelected(detail); setDraft(JSON.stringify(detail, null, 2)); setIsNew(false)
    } catch (error) {
      onMessage(error instanceof Error ? error.message : 'İçerik açılamadı.')
    } finally { setBusy(false) }
  }

  const createNew = () => {
    setSelected(null); setDraft(JSON.stringify(templates[kind], null, 2)); setIsNew(true)
  }

  const save = async (event: FormEvent) => {
    event.preventDefault()
    let payload: ContentItem
    try { payload = JSON.parse(draft) as ContentItem }
    catch { onMessage('JSON biçimi geçersiz. Virgül ve tırnakları kontrol edin.'); return }
    setBusy(true)
    try {
      const path = isNew ? `/admin/content/${kind}/` : `/admin/content/${kind}/${payload.stableId}/${payload.version}`
      const saved = await request<ContentItem>(path, { method: isNew ? 'POST' : 'PUT', body: JSON.stringify(payload) })
      onMessage(`${saved.title ?? saved.prompt ?? saved.stableId} kaydedildi.`)
      await load()
      await selectItem(saved)
    } catch (error) {
      onMessage(error instanceof Error ? error.message : 'İçerik kaydedilemedi.')
    } finally { setBusy(false) }
  }

  const remove = async () => {
    if (!selected || !window.confirm(`${selected.title ?? selected.prompt ?? selected.stableId} silinsin mi?`)) return
    setBusy(true)
    try {
      await request(`/admin/content/${kind}/${selected.stableId}/${selected.version}`, { method: 'DELETE' })
      onMessage('İçerik silindi.'); await load()
    } catch (error) {
      onMessage(error instanceof Error ? error.message : 'İçerik silinemedi.')
    } finally { setBusy(false) }
  }

  const transition = async (targetStatus: string, label: string) => {
    if (!selected) return
    setBusy(true)
    try {
      await request(`/admin/content/${kind}/${selected.stableId}/${selected.version}/transitions`, {
        method: 'POST', body: JSON.stringify({ targetStatus }),
      })
      onMessage(`${selected.title ?? selected.prompt ?? selected.stableId}: ${label}.`)
      await load()
      await selectItem({ ...selected, status: targetStatus })
    } catch (error) {
      onMessage(error instanceof Error ? error.message : 'Durum değiştirilemedi.')
    } finally { setBusy(false) }
  }

  return (
    <section className="admin-studio">
      <header className="admin-hero">
        <div><span className="eyebrow">YÖNETİM / İÇERİK MASASI</span><h1>Bilgiyi <em>yayına</em> hazırla.</h1></div>
        <p>Kaynağı, sürümü ve yayın durumunu tek yerde düzenleyin. Değişiklikler kaydedilmeden önce API kurallarıyla doğrulanır.</p>
      </header>
      <div className="admin-kind-tabs" role="tablist" aria-label="İçerik türleri">
        {kinds.map(item => <button key={item.id} role="tab" aria-selected={kind === item.id} onClick={() => setKind(item.id)}>
          <span>{item.code}</span>{item.label}<b>{kind === item.id ? items.length : '—'}</b>
        </button>)}
      </div>
      <div className="admin-workbench">
        <aside className="admin-index">
          <div className="admin-index-head">
            <label>İçerikte ara<input value={filter} onChange={event => setFilter(event.target.value)} placeholder="Başlık veya stable ID" /></label>
            <button className="primary" onClick={createNew}>Yeni {meta.singular}</button>
          </div>
          <div className="admin-ledger" aria-busy={busy}>
            {visibleItems.map((item, index) => <button key={`${item.stableId}-${item.version}`} className={selected?.stableId === item.stableId && selected.version === item.version ? 'active' : ''} onClick={() => selectItem(item)}>
              <span>{String(index + 1).padStart(2, '0')}</span>
              <span><b>{item.title ?? item.prompt}</b><small>{item.stableId} · v{item.version}</small></span>
              <i data-status={item.status}>{item.status}</i>
            </button>)}
            {!busy && visibleItems.length === 0 && <div className="admin-empty">Bu görünümde içerik yok.<br />Yeni bir {meta.singular} oluşturun.</div>}
          </div>
        </aside>
        <section className="admin-editor">
          {draft ? <form onSubmit={save}>
            <header>
              <div><span>{isNew ? 'YENİ KAYIT' : 'SÜRÜM DETAYI'}</span><h2>{isNew ? `Yeni ${meta.singular}` : selected?.title ?? selected?.prompt}</h2></div>
              <div className="admin-actions">
                {!isNew && <button type="button" className="admin-delete" onClick={remove}>Sil</button>}
                <button className="primary" disabled={busy}>Değişiklikleri kaydet</button>
              </div>
            </header>
            {!isNew && selected && <div className="workflow-bar" aria-label="Yayın akışı">
              <div><small>MEVCUT DURUM</small><b>{selected.status}</b></div>
              <span aria-hidden="true">→</span>
              <div className="workflow-actions">
                {selected.status === 'draft' && <button type="button" onClick={() => transition('inReview', 'İncelemeye gönderildi')}>İncelemeye gönder</button>}
                {selected.status === 'inReview' && <button type="button" onClick={() => transition('draft', 'Taslağa döndürüldü')}>Taslağa döndür</button>}
                {selected.status === 'inReview' && canPublish && <button type="button" className="publish" onClick={() => transition('published', 'Yayınlandı')}>Yayınla</button>}
                {selected.status === 'published' && canPublish && <button type="button" onClick={() => transition('archived', 'Arşivlendi')}>Arşivle</button>}
                {selected.status === 'archived' && <button type="button" onClick={() => transition('draft', 'Yeni taslak açıldı')}>Taslağa geri al</button>}
              </div>
            </div>}
            <label className="json-field">İçerik sözleşmesi<textarea value={draft} onChange={event => setDraft(event.target.value)} rows={26} spellCheck={false} aria-describedby="json-help" /></label>
            <p id="json-help" className="admin-help">Alan adlarını koruyun. Bölüm ve rubric sıraları benzersiz, rubric ağırlıkları toplamı 100 olmalıdır.</p>
          </form> : <div className="admin-editor-empty"><span>{meta.code}</span><h2>Bir kayıt seçin</h2><p>İçeriği incelemek için soldaki dizinden seçim yapın veya yeni bir {meta.singular} başlatın.</p></div>}
        </section>
      </div>
    </section>
  )
}
