'use client';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';

export default function NewPatientPage() {
  const router = useRouter();
  const [form, setForm] = useState({ firstName: '', lastName: '', phoneNumber: '', primaryBranchId: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true); setError('');
    try {
      await api.createPatient({
        firstName: form.firstName,
        lastName: form.lastName,
        phoneNumber: form.phoneNumber,
        primaryBranchId: form.primaryBranchId || undefined,
      });
      router.push('/patients');
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  const field = (label: string, key: keyof typeof form, type = 'text', required = true) => (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
      <label style={{ fontWeight: 500, fontSize: 14 }}>{label}{required && ' *'}</label>
      <input type={type} value={form[key]} required={required}
        onChange={e => setForm(f => ({ ...f, [key]: e.target.value }))}
        style={{ padding: '10px 12px', borderRadius: 6, border: '1px solid #d1d5db', fontSize: 14 }} />
    </div>
  );

  return (
    <div style={{ maxWidth: 480 }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 24 }}>
        <a href="/patients" style={{ color: '#6b7280', textDecoration: 'none' }}>← Patients</a>
        <h2 style={{ margin: 0 }}>New Patient</h2>
      </div>

      {error && (
        <div style={{ color: '#dc2626', padding: 12, background: '#fef2f2', borderRadius: 6, marginBottom: 16, border: '1px solid #fecaca' }}>
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} style={{ background: 'white', padding: 24, borderRadius: 8, boxShadow: '0 1px 3px rgba(0,0,0,0.1)', display: 'flex', flexDirection: 'column', gap: 16 }}>
        {field('First Name', 'firstName')}
        {field('Last Name', 'lastName')}
        {field('Phone Number', 'phoneNumber', 'tel')}
        {field('Primary Branch ID', 'primaryBranchId', 'text', false)}

        <div style={{ display: 'flex', gap: 12, marginTop: 8 }}>
          <button type="submit" disabled={loading}
            style={{ flex: 1, padding: '11px', background: '#1a56db', color: 'white', border: 'none', borderRadius: 6, cursor: 'pointer', fontSize: 15, fontWeight: 500 }}>
            {loading ? 'Creating...' : 'Create Patient'}
          </button>
          <a href="/patients"
            style={{ padding: '11px 20px', background: '#f3f4f6', color: '#374151', borderRadius: 6, textDecoration: 'none', fontSize: 15, textAlign: 'center' }}>
            Cancel
          </a>
        </div>
      </form>
    </div>
  );
}
