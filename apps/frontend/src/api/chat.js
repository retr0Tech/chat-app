const API = process.env.REACT_APP_API_URL || 'http://localhost:5006';

export async function getChatRooms(token) {
  const res = await fetch(`${API}/api/chatrooms`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error('Failed to fetch rooms');
  return res.json();
}

export async function getMessages(token, roomId) {
  const res = await fetch(`${API}/api/chatrooms/${roomId}/messages`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error('Failed to fetch messages');
  return res.json();
}
