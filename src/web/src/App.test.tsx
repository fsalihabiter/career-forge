import { render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'

describe('App', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn(async () =>
      new Response(JSON.stringify([]), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })))
  })

  it('renders the account entry screen and loads the public catalog', async () => {
    render(<App />)

    expect(screen.getByRole('heading', {
      name: /Mülakatta bildiğini değil, nasıl düşündüğünü göster/i,
    })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Yetkinliklerimi seç' })).toBeInTheDocument()

    await waitFor(() => {
      expect(fetch).toHaveBeenCalledTimes(3)
    })
    expect(fetch).toHaveBeenCalledWith(
      expect.stringMatching(/\/technologies$/),
      expect.any(Object),
    )
    expect(fetch).toHaveBeenCalledWith(
      expect.stringMatching(/\/skills$/),
      expect.any(Object),
    )
    expect(fetch).toHaveBeenCalledWith(
      expect.stringMatching(/\/specializations$/),
      expect.any(Object),
    )
  })
})
