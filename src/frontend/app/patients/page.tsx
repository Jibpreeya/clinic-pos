'use client';
import { useState, useEffect, useCallback } from 'react';
import { api } from '@/lib/api';

interface Patient {
  id: string; firstName: string; lastName: string;
  phoneNumber: string; primaryBranchName?: string; createdAt: string;
}

export default function PatientsPage() {
  const [patients, setPatients] = useState<Patient[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [branchId, setBranchId] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [role, setRole] = useState('');

  useEffect(() => { setRole(localStorage.getItem('role') || ''); }, []);

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try {
      const res = await api.getPatients({ branchId: branchId || undefined, page });
      setPatients(res.items); setTotal(res.total);
    } catch (e: any) { setError(e.message); }
    finally { setLoading(false); }
  }, [branchId, page]);

  useEffect(() => { load(); }, [load]);

  const canCreate = role === 'Admin' || role === 'User';

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h2>Patients <span style={{ color: '#6b7280', fontSize: 16 }}>({total} total)</span></h2>
        {canCreate && (
          <a href="/patients/new" style={{ padding: '8px 16px', background: '#1a56db', color: 'white', borderRadius: 6, textDecoration: 'none' }}>
            + New Patient
          </a>
        )}
      </div>

      <div style={{ marginBottom: 16 }}>
        <input placeholder="Filter by Branch ID (optional)" value={branchId}
          onChange={e => { setBranchId(e.target.value); setPage(1); }}
          style={{ padding: '8px 12px', borderRadius: 6, border: '1px solid #d1d5db', width: 280 }} />
      </div>

      {error && <div style={{ color: 'red', padding: 8, background: '#fee', borderRadius: 4 }}>{error}</div>}
      {loading ? <p>Loading...</p> : (
        <table style={{ width: '100%', borderCollapse: 'collapse', background: 'white', borderRadius: 8, overflow: 'hidden', boxShadow: '0 1px 3px rgba(0,0,0,0.1)' }}>
          <thead style={{ background: '#f9fafb' }}>
            <tr>
              {['Name', 'Phone', 'Primary Branch', 'Created'].map(h => (
                <th key={h} style={{ padding: '12px 16px', textAlign: 'left', fontWeight: 600, fontSize: 13, color: '#374151' }}>{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {patients.length === 0 ? (
              <tr><td colSpan={4} style={{ padding: 32, textAlign: 'center', color: '#9ca3af' }}>No patients found.</td></tr>
            ) : patients.map((p, i) => (
              <tr key={p.id} style={{ borderTop: '1px solid #f3f4f6', background: i % 2 === 0 ? 'white' : '#fafafa' }}>
                <td style={{ padding: '12px 16px' }}>{p.firstName} {p.lastName}</td>
                <td style={{ padding: '12px 16px' }}>{p.phoneNumber}</td>
                <td style={{ padding: '12px 16px', color: '#6b7280' }}>{p.primaryBranchName || '—'}</td>
                <td style={{ padding: '12px 16px', color: '#9ca3af', fontSize: 13 }}>
                  {new Date(p.createdAt).toLocaleDateString()}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {total > 20 && (
        <div style={{ marginTop: 16, display: 'flex', gap: 8 }}>
          <button onClick={() => setPage(p => Math.max(1, p - 1))} disabled={page === 1} style={btnStyle}>← Prev</button>
          <span style={{ padding: '8px 12px' }}>Page {page}</span>
          <button onClick={() => setPage(p => p + 1)} disabled={page * 20 >= total} style={btnStyle}>Next →</button>
        </div>
      )}
    </div>
  );
}

const btnStyle: React.CSSProperties = {
  padding: '8px 14px', background: '#f3f4f6', border: '1px solid #d1d5db',
  borderRadius: 6, cursor: 'pointer'
};
