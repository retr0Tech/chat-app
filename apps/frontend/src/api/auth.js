const API = process.env.REACT_APP_API_URL || 'http://localhost:5006';

export async function login(email, password) {
  const res = await fetch(`${API}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) throw new Error('Invalid credentials');
  return res.json();
}

export async function register(email, username, password) {
  const res = await fetch(`${API}/api/auth/register`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, username, password }),
  });
  if (!res.ok) {
    const data = await res.json();
    const msg = Array.isArray(data) ? data.map(e => e.description).join(', ') : 'Registration failed';
    throw new Error(msg);
  }
  return res.json();
}
