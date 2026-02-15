export default function Home() {
  return (
    <div>
      <h1>Welcome to Clinic POS</h1>
      <p>Multi-tenant clinic management system.</p>
      <div style={{ display: 'flex', gap: 16, marginTop: 24 }}>
        <a href="/login" style={{ padding: '10px 20px', background: '#1a56db', color: 'white', borderRadius: 6, textDecoration: 'none' }}>
          Login
        </a>
        <a href="/patients" style={{ padding: '10px 20px', background: '#374151', color: 'white', borderRadius: 6, textDecoration: 'none' }}>
          View Patients
        </a>
      </div>
    </div>
  );
}
