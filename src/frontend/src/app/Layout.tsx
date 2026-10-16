import { Outlet } from 'react-router'

export function Layout() {
  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-white px-6 py-4">
        <p className="text-lg font-semibold">Shop Management</p>
      </header>
      <main className="px-6 py-8">
        <Outlet />
      </main>
    </div>
  )
}
