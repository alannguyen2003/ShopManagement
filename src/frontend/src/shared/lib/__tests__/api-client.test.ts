import { afterEach, describe, expect, it, vi } from 'vitest'
import { ApiError, apiRequest } from '@/shared/lib/api-client'

function stubFetch(response: Response) {
  const fetchMock = vi.fn().mockResolvedValue(response)
  vi.stubGlobal('fetch', fetchMock)
  return fetchMock
}

afterEach(() => {
  vi.unstubAllGlobals()
})

describe('apiRequest', () => {
  it('returns the parsed JSON body on success', async () => {
    stubFetch(Response.json({ status: 'healthy' }))

    await expect(apiRequest('/health')).resolves.toEqual({ status: 'healthy' })
  })

  it('sends JSON headers when a body is given', async () => {
    const fetchMock = stubFetch(Response.json({}))

    await apiRequest('/api/items', { method: 'POST', body: '{}' })

    const headers = fetchMock.mock.calls[0][1].headers as Headers
    expect(headers.get('Accept')).toBe('application/json')
    expect(headers.get('Content-Type')).toBe('application/json')
  })

  it('returns undefined for 204 No Content', async () => {
    stubFetch(new Response(null, { status: 204 }))

    await expect(apiRequest('/api/items/1', { method: 'DELETE' })).resolves.toBeUndefined()
  })

  it('throws ApiError with the ProblemDetails body on failure', async () => {
    const problem = { title: 'Not Found', status: 404 }
    stubFetch(Response.json(problem, { status: 404 }))

    const error = await apiRequest('/api/missing').catch((e: unknown) => e)

    expect(error).toBeInstanceOf(ApiError)
    expect(error).toMatchObject({ status: 404, problem, message: 'Not Found' })
  })

  it('throws ApiError with a fallback problem when the body is not JSON', async () => {
    stubFetch(new Response('boom', { status: 500, statusText: 'Internal Server Error' }))

    const error = await apiRequest('/api/broken').catch((e: unknown) => e)

    expect(error).toMatchObject({ status: 500, problem: { status: 500, title: 'Internal Server Error' } })
  })
})
