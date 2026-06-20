import { useState } from 'react';
import { apiFetch } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useToast } from '../context/ToastContext.jsx';

function AuthLogo() {
  return (
    <div style={{ textAlign: 'center', marginBottom: 32 }}>
      <div style={{ fontFamily: 'var(--font-display)', fontSize: '1.75rem', fontWeight: 800, marginBottom: 4 }}>
        <span style={{ color: 'var(--pulse)' }}>Bid</span>Pulse
      </div>
    </div>
  );
}

export function LoginPage({ setPage }) {
  const { login } = useAuth();
  const toast = useToast();
  const [form, setForm] = useState({ email: '', password: '' });
  const [loading, setLoading] = useState(false);

  const submit = async () => {
    setLoading(true);
    try {
      await login(form.email, form.password);
      setPage('home');
    } catch (e) {
      toast(e.message, 'error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page-sm" style={{ margin: '0 auto', padding: '60px 24px' }}>
      <AuthLogo />
      <div className="auth-card">
        <div className="auth-title">Welcome back</div>
        <div className="auth-sub">Enter your credentials to continue.</div>
        <div className="auth-form">
          <div className="field">
            <label>Email</label>
            <input
              type="email" placeholder="you@example.com"
              value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
              onKeyDown={e => e.key === 'Enter' && submit()}
            />
          </div>
          <div className="field">
            <label>Password</label>
            <input
              type="password" placeholder="••••••••"
              value={form.password} onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
              onKeyDown={e => e.key === 'Enter' && submit()}
            />
          </div>
          <button className="btn btn-primary btn-lg" style={{ width: '100%', marginTop: 4 }}
            onClick={submit} disabled={loading || !form.email || !form.password}>
            {loading ? <span className="spinner" /> : 'Sign In'}
          </button>
        </div>
        <div className="auth-switch">
          Don't have an account? <a onClick={() => setPage('register')}>Sign up</a>
        </div>
      </div>
    </div>
  );
}

export function RegisterPage({ setPage }) {
  const { login } = useAuth();
  const toast = useToast();
  const [form, setForm] = useState({ nickName: '', email: '', password: '' });
  const [loading, setLoading] = useState(false);

  const submit = async () => {
    setLoading(true);
    try {
      await apiFetch('/auth/register', { method: 'POST', body: JSON.stringify(form) });
      toast('Account created! Signing you in…', 'success');
      await login(form.email, form.password);
      setPage('home');
    } catch (e) {
      toast(e.message, 'error');
    } finally {
      setLoading(false);
    }
  };

  const isValid = form.nickName && form.email && form.password;

  return (
    <div className="page-sm" style={{ margin: '0 auto', padding: '60px 24px' }}>
      <AuthLogo />
      <div className="auth-card">
        <div className="auth-title">Get started</div>
        <div className="auth-sub">Join thousands of bidders on BidPulse.</div>
        <div className="auth-form">
          <div className="field">
            <label>Nickname</label>
            <input placeholder="coolbidder99" value={form.nickName} onChange={e => setForm(f => ({ ...f, nickName: e.target.value }))} />
          </div>
          <div className="field">
            <label>Email</label>
            <input type="email" placeholder="you@example.com" value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} />
          </div>
          <div className="field">
            <label>Password</label>
            <input
              type="password" placeholder="Min 8 characters"
              value={form.password} onChange={e => setForm(f => ({ ...f, password: e.target.value }))}
              onKeyDown={e => e.key === 'Enter' && submit()}
            />
          </div>
          <button className="btn btn-primary btn-lg" style={{ width: '100%', marginTop: 4 }}
            onClick={submit} disabled={loading || !isValid}>
            {loading ? <span className="spinner" /> : 'Create Account'}
          </button>
        </div>
        <div className="auth-switch">
          Already have an account? <a onClick={() => setPage('login')}>Sign in</a>
        </div>
      </div>
    </div>
  );
}