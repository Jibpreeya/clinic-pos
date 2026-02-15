'use client';
import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';

export default function NewAppointmentPage() {
  const router = useRouter();
  const [form, setForm] = useState({ branchId: '', patientId: '', startAt: '' });
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true); setError('');
    try {
      await api.createAppointment({
        branchId: form.branchId,
        patientId: form.patientId,
        startAt: new Date(form.startAt).toISOString(),
      });
      router.push('/patients');
    } catch (err: any) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div style={{ maxWidth: 480 }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginBottom: 24 }}>
        <a href="/patients" style={{ color: '#6b7280', textDecoration: 'none' }}>← Back</a>
        <h2 style={{ margin: 0 }}>New Appointment</h2>
      </div>

      {error && (
        <div style={{ color: '#dc2626', padding: 12, background: '#fef2f2', borderRadius: 6, marginBottom: 16, border: '1px solid #fecaca' }}>
          {error}
        </div>
      )}

      <form onSubmit={handleSubmit} style={{ background: 'white', padding: 24, borderRadius: 8, boxShadow: '0 1px 3px rgba(0,0,0,0.1)', display: 'flex', flexDirection: 'column', gap: 16 }}>
        {[
          { label: 'Branch ID *', key: 'branchId', type: 'text' },
          { label: 'Patient ID *', key: 'patientId', type: 'text' },
          { label: 'Date & Time *', key: 'startAt', type: 'datetime-local' },
        ].map(({ label, key, type }) => (
          <div key={key} style={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
            <label style={{ fontWeight: 500, fontSize: 14 }}>{label}</label>
            <input type={type} required value={(form as any)[key]}
              onChange={e => setForm(f => ({ ...f, [key]: e.target.value }))}
              style={{ padding: '10px 12px', borderRadius: 6, border: '1px solid #d1d5db', fontSize: 14 }} />
          </div>
        ))}

        <button type="submit" disabled={loading}
          style={{ padding: '11px', background: '#1a56db', color: 'white', border: 'none', borderRadius: 6, cursor: 'pointer', fontSize: 15, fontWeight: 500 }}>
          {loading ? 'Booking...' : 'Book Appointment'}
        </button>
      </form>
    </div>
  );
}
