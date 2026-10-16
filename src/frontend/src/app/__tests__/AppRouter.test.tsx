import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { AppRouter } from '@/app/router'

describe('AppRouter', () => {
  it('renders the layout and the home page at /', () => {
    render(<AppRouter />)

    expect(screen.getByRole('banner')).toHaveTextContent('Shop Management')
    expect(screen.getByRole('heading', { name: 'Welcome' })).toBeInTheDocument()
  })
})
