import { useState, useEffect } from 'react';
import { apiFetch } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useToast } from '../context/ToastContext.jsx';

export function ProfilePage() {
  const { user, refreshUser } = useAuth();
  const toast = useToast();
  const [form, setForm] = useState({ nickName: user?.nickName || '', email: user?.email || '' });
  const [saving, setSaving] = useState(false);
  const [myBids, setMyBids] = useState([]);
  const [tab, setTab] = useState('bids');

  useEffect(() => {
    if (user?.id) {
      apiFetch(`/bids/user/${user.id}`).then(setMyBids).catch(() => {});
    }
    setForm({ nickName: user?.nickName || '', email: user?.email || '' });
  }, [user]);

  const saveProfile = async () => {
    setSaving(true);
    try {
      await apiFetch(`/users/${user.id}/profile`, { method: 'PUT', body: JSON.stringify(form) });
      toast('Profile updated!', 'success');
      refreshUser();
    } catch (e) {
      toast(e.message, 'error');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="page page-md">
      <div className="profile-header">
        <div className="profile-avatar">{user?.nickName?.[0]?.toUpperCase() || 'U'}</div>
        <div>
          <div className="profile-name">{user?.nickName}</div>
          <div className="profile-email">{user?.email}</div>
          <div className="profile-balance">${Number(user?.balance || 0).toFixed(2)}</div>
          <div style={{ marginTop: 8 }}>
            <span className={`badge badge-${(user?.role || 'user').toLowerCase()}`}>{user?.role}</span>
          </div>
        </div>
      </div>

      <div className="tabs">
        <button className={`tab ${tab === 'bids' ? 'active' : ''}`} onClick={() => setTab('bids')}>My Bids</button>
        <button className={`tab ${tab === 'edit' ? 'active' : ''}`} onClick={() => setTab('edit')}>Edit Profile</button>
      </div>

      {tab === 'bids' && (
        <div className="card">
          <div className="card-body">
            {myBids.length === 0 ? (
              <div className="empty-state" style={{ padding: '30px 0' }}>
                <div style={{ fontSize: '1.5rem', marginBottom: 8 }}>🎯</div>
                <p>You haven't placed any bids yet.</p>
              </div>
            ) : (
              <div className="table-wrap">
                <table>
                  <thead><tr><th>Amount</th><th>Lot</th><th>Date</th></tr></thead>
                  <tbody>
                    {[...myBids].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).map(b => (
                      <tr key={b.id}>
                        <td><span className="mono" style={{ fontWeight: 700 }}>${Number(b.amount).toFixed(2)}</span></td>
                        <td className="mono" style={{ color: 'var(--muted)', fontSize: '.8125rem' }}>{b.lotId.slice(0, 12)}…</td>
                        <td style={{ color: 'var(--muted)', fontSize: '.8125rem' }}>{new Date(b.createdAt).toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      {tab === 'edit' && (
        <div className="card">
          <div className="card-body" style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
            <div className="field">
              <label>Nickname</label>
              <input value={form.nickName} onChange={e => setForm(f => ({ ...f, nickName: e.target.value }))} />
            </div>
            <div className="field">
              <label>Email</label>
              <input type="email" value={form.email} onChange={e => setForm(f => ({ ...f, email: e.target.value }))} />
            </div>
            <div style={{ display: 'flex', justifyContent: 'flex-end' }}>
              <button className="btn btn-primary" onClick={saveProfile} disabled={saving}>
                {saving ? <span className="spinner" /> : 'Save Changes'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}