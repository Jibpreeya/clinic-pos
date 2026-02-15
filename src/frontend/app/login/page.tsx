'use client';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';

export default function LoginPage() {
  const router = useRouter();
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true); setError('');
    try {
      const res = await api.login(form.email, form.password);
      localStorage.setItem('token', res.token);
      localStorage.setItem('tenantId', res.tenantId);
      localStorage.setItem('role', res.role);
      router.push('/patients');
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ maxWidth: 400, margin: '80px auto' }}>
      <h2>Login</h2>
      {error && <div style={{ color: 'red', marginBottom: 12, padding: 8, background: '#fee', borderRadius: 4 }}>{error}</div>}
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 12 }}>
        <input placeholder="Email" type="email" value={form.email}
          onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
          style={inputStyle} required />
        <input placeholder="Password" type="password" value={form.password}
          onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
          style={inputStyle} required />
        <button type="submit" disabled={loading} style={btnStyle}>
          {loading ? 'Logging in...' : 'Login'}
        </button>
      </form>
      <p style={{ marginTop: 16, color: '#555', fontSize: 13 }}>
        Demo: admin@demo.com / Admin1234! &nbsp;|&nbsp; viewer@demo.com / Viewer1234!
      </p>
    </div>
  );
}

const inputStyle: React.CSSProperties = {
  padding: '10px 12px', borderRadius: 6, border: '1px solid #d1d5db', fontSize: 14
};
const btnStyle: React.CSSProperties = {
  padding: '10px', background: '#1a56db', color: 'white', border: 'none',
  borderRadius: 6, cursor: 'pointer', fontSize: 15
};
