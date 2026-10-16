import { createBrowserRouter, RouterProvider } from 'react-router'
import { HomePage } from '@/app/HomePage'
import { Layout } from '@/app/Layout'

const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [{ index: true, element: <HomePage /> }],
  },
])

export function AppRouter() {
  return <RouterProvider router={router} />
}
