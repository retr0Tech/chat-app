import { useState } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/LoginPage';
import ChatPage from './pages/ChatPage';
import './App.css';

function App() {
  const [auth, setAuth] = useState(() => {
    const saved = sessionStorage.getItem('auth');
    return saved ? JSON.parse(saved) : null;
  });

  const handleLogin = (data) => {
    setAuth(data);
    sessionStorage.setItem('auth', JSON.stringify(data));
  };

  const handleLogout = () => {
    setAuth(null);
    sessionStorage.removeItem('auth');
  };

  return (
    <BrowserRouter>
      <Routes>
        <Route
          path="/"
          element={auth ? <Navigate to="/chat" /> : <LoginPage onLogin={handleLogin} />}
        />
        <Route
          path="/chat"
          element={auth ? <ChatPage auth={auth} onLogout={handleLogout} /> : <Navigate to="/" />}
        />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
