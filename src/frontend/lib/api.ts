const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

function getToken() {
  if (typeof window === 'undefined') return null;
  return localStorage.getItem('token');
}

async function apiFetch(path: string, options: RequestInit = {}) {
  const token = getToken();
  const res = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });
  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(err.error || 'Request failed');
  }
  return res.json();
}

export const api = {
  login: (email: string, password: string) =>
    apiFetch('/api/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),

  getPatients: (params?: { branchId?: string; page?: number }) => {
    const q = new URLSearchParams();
    if (params?.branchId) q.set('branchId', params.branchId);
    if (params?.page) q.set('page', String(params.page));
    return apiFetch(`/api/patients?${q}`);
  },

  createPatient: (data: { firstName: string; lastName: string; phoneNumber: string; primaryBranchId?: string }) =>
    apiFetch('/api/patients', { method: 'POST', body: JSON.stringify(data) }),

  createAppointment: (data: { branchId: string; patientId: string; startAt: string }) =>
    apiFetch('/api/appointments', { method: 'POST', body: JSON.stringify(data) }),
};
