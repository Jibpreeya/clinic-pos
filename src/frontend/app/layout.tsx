import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'Clinic POS',
  description: 'Multi-tenant Clinic Management System',
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body style={{ fontFamily: 'system-ui, sans-serif', margin: 0, background: '#f5f5f5' }}>
        <nav style={{ background: '#1a56db', padding: '12px 24px', color: 'white', display: 'flex', gap: 24 }}>
          <strong>Clinic POS</strong>
          <a href="/patients" style={{ color: 'white', textDecoration: 'none' }}>Patients</a>
          <a href="/appointments/new" style={{ color: 'white', textDecoration: 'none' }}>New Appointment</a>
        </nav>
        <main style={{ maxWidth: 900, margin: '32px auto', padding: '0 16px' }}>
          {children}
        </main>
      </body>
    </html>
  );
}
