import { createContext, useContext, useState, useEffect } from 'react';
import { apiFetch } from '../api/client.js';

const AuthCtx = createContext(null);

export function useAuth() {
  return useContext(AuthCtx);
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const token = localStorage.getItem('bp_token');
    const uid = localStorage.getItem('bp_uid');
    if (token && uid) {
      apiFetch(`/users/${uid}`)
        .then(setUser)
        .catch(() => localStorage.clear())
        .finally(() => setLoading(false));
    } else {
      setLoading(false);
    }
  }, []);

  const login = async (email, password) => {
    const data = await apiFetch('/auth/login', {
      method: 'POST',
      body: JSON.stringify({ email, password }),
    });
    localStorage.setItem('bp_token', data.token);
    localStorage.setItem('bp_uid', data.userId);
    const u = await apiFetch(`/users/${data.userId}`);
    setUser(u);
  };

  const logout = () => {
    localStorage.removeItem('bp_token');
    localStorage.removeItem('bp_uid');
    setUser(null);
  };

  const refreshUser = () => {
    const uid = localStorage.getItem('bp_uid');
    if (uid) apiFetch(`/users/${uid}`).then(setUser).catch(() => {});
  };

  return (
    <AuthCtx.Provider value={{ user, login, logout, refreshUser, loading }}>
      {children}
    </AuthCtx.Provider>
  );
}