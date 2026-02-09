import { useState, useEffect, useRef, useCallback } from 'react';
import { getChatRooms, getMessages } from '../api/chat';
import { createChatConnection } from '../api/signalr';

export default function ChatPage({ auth, onLogout }) {
  const [rooms, setRooms] = useState([]);
  const [activeRoom, setActiveRoom] = useState(null);
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState('');
  const [connection, setConnection] = useState(null);
  const messagesEndRef = useRef(null);

  // Load rooms on mount
  useEffect(() => {
    getChatRooms(auth.token).then(setRooms).catch(console.error);
  }, [auth.token]);

  // Set up SignalR connection
  useEffect(() => {
    const conn = createChatConnection(auth.token);
    conn.on('ReceiveMessage', (msg) => {
      setMessages((prev) => [...prev, msg]);
    });
    conn.start().then(() => setConnection(conn)).catch(console.error);
    return () => { conn.stop(); };
  }, [auth.token]);

  // Join room
  const joinRoom = useCallback(async (roomId) => {
    if (!connection) return;
    if (activeRoom) {
      await connection.invoke('LeaveRoom', activeRoom);
    }
    await connection.invoke('JoinRoom', roomId);
    const msgs = await getMessages(auth.token, roomId);
    setMessages(msgs);
    setActiveRoom(roomId);
  }, [connection, activeRoom, auth.token]);

  // Auto-scroll
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Auto-join first room
  useEffect(() => {
    if (connection && rooms.length > 0 && !activeRoom) {
      joinRoom(rooms[0].id);
    }
  }, [connection, rooms, activeRoom, joinRoom]);

  const sendMessage = async (e) => {
    e.preventDefault();
    if (!input.trim() || !connection || !activeRoom) return;
    await connection.invoke('SendMessage', activeRoom, input);
    setInput('');
  };

  return (
    <div className="chat-container">
      <div className="sidebar">
        <div className="sidebar-header">
          <strong>{auth.username}</strong>
          <button onClick={onLogout}>Logout</button>
        </div>
        <h3>Rooms</h3>
        {rooms.map((r) => (
          <button
            key={r.id}
            className={`room-btn ${activeRoom === r.id ? 'active' : ''}`}
            onClick={() => joinRoom(r.id)}
          >
            {r.name}
          </button>
        ))}
      </div>
      <div className="chat-main">
        <div className="messages">
          {messages.map((m, i) => (
            <div key={m.id || i} className={`message ${m.isBot ? 'bot' : ''}`}>
              <strong>{m.username}</strong>
              <span className="time">
                {new Date(m.createdAt).toLocaleTimeString()}
              </span>
              <p>{m.content}</p>
            </div>
          ))}
          <div ref={messagesEndRef} />
        </div>
        <form className="message-input" onSubmit={sendMessage}>
          <input
            type="text"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Type a message or /stock=AAPL.US"
          />
          <button type="submit">Send</button>
        </form>
      </div>
    </div>
  );
}
