import { useState, useEffect, useCallback } from 'react';
import { apiFetch } from '../api/client.js';
import { useToast } from '../context/ToastContext.jsx';

export function AdminPage() {
  const toast = useToast();
  const [tab, setTab] = useState('users');
  const [users, setUsers] = useState([]);
  const [cats, setCats] = useState([]);
  const [loading, setLoading] = useState(true);
  const [newCat, setNewCat] = useState('');
  const [catDesc, setCatDesc] = useState('');
  const [roleModal, setRoleModal] = useState(null);
  const [newRole, setNewRole] = useState('');

  const refresh = useCallback(() => {
    setLoading(true);
    Promise.all([apiFetch('/users'), apiFetch('/categories')])
      .then(([u, c]) => { setUsers(u); setCats(c); })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  useEffect(() => { refresh(); }, [refresh]);

  const createCat = async () => {
    try {
      await apiFetch('/categories', { method: 'POST', body: JSON.stringify({ name: newCat, description: catDesc }) });
      toast('Category created!', 'success');
      setNewCat(''); setCatDesc('');
      refresh();
    } catch (e) { toast(e.message, 'error'); }
  };

  const deleteCat = async (id) => {
    if (!confirm('Delete this category?')) return;
    try {
      await apiFetch(`/categories/${id}`, { method: 'DELETE' });
      toast('Category deleted.', 'success');
      refresh();
    } catch (e) { toast(e.message, 'error'); }
  };

  const deleteUser = async (id) => {
    if (!confirm('Delete this user?')) return;
    try {
      await apiFetch(`/users/${id}`, { method: 'DELETE' });
      toast('User deleted.', 'success');
      refresh();
    } catch (e) { toast(e.message, 'error'); }
  };

  const changeRole = async () => {
    try {
      await apiFetch(`/users/${roleModal.id}/role`, { method: 'PUT', body: JSON.stringify({ role: newRole }) });
      toast('Role updated!', 'success');
      setRoleModal(null);
      refresh();
    } catch (e) { toast(e.message, 'error'); }
  };

  return (
    <div className="page">
      <div className="section-title">Admin Panel</div>
      <div className="section-sub">Manage users, categories, and platform settings.</div>

      <div className="tabs">
        <button className={`tab ${tab === 'users' ? 'active' : ''}`} onClick={() => setTab('users')}>
          Users ({users.length})
        </button>
        <button className={`tab ${tab === 'cats' ? 'active' : ''}`} onClick={() => setTab('cats')}>
          Categories ({cats.length})
        </button>
      </div>

      {loading ? (
        <div className="loading-page"><div className="spinner spinner-dark" /></div>
      ) : (
        <>
          {tab === 'users' && (
            <div className="card">
              <div className="card-body">
                <div className="table-wrap">
                  <table>
                    <thead>
                      <tr><th>Nickname</th><th>Email</th><th>Role</th><th>Balance</th><th>Joined</th><th></th></tr>
                    </thead>
                    <tbody>
                      {users.map(u => (
                        <tr key={u.id}>
                          <td style={{ fontWeight: 600 }}>{u.nickName}</td>
                          <td style={{ color: 'var(--muted)' }}>{u.email}</td>
                          <td><span className={`badge badge-${u.role.toLowerCase()}`}>{u.role}</span></td>
                          <td className="mono">${Number(u.balance).toFixed(2)}</td>
                          <td style={{ color: 'var(--muted)', fontSize: '.8125rem' }}>{new Date(u.createdAt).toLocaleDateString()}</td>
                          <td>
                            <div style={{ display: 'flex', gap: 6 }}>
                              <button className="btn btn-secondary btn-sm" onClick={() => { setRoleModal(u); setNewRole(u.role); }}>Role</button>
                              <button className="btn btn-danger btn-sm" onClick={() => deleteUser(u.id)}>Delete</button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            </div>
          )}

          {tab === 'cats' && (
            <>
              <div className="card" style={{ marginBottom: 20 }}>
                <div className="card-body">
                  <div style={{ fontWeight: 700, marginBottom: 12 }}>Create Category</div>
                  <div className="form-grid">
                    <div className="field">
                      <label>Name</label>
                      <input placeholder="Category name" value={newCat} onChange={e => setNewCat(e.target.value)} />
                    </div>
                    <div className="field">
                      <label>Description</label>
                      <input placeholder="Optional description" value={catDesc} onChange={e => setCatDesc(e.target.value)} />
                    </div>
                  </div>
                  <div style={{ display: 'flex', justifyContent: 'flex-end', marginTop: 12 }}>
                    <button className="btn btn-primary" onClick={createCat} disabled={!newCat}>Create</button>
                  </div>
                </div>
              </div>

              <div className="card">
                <div className="card-body">
                  <div className="table-wrap">
                    <table>
                      <thead><tr><th>Name</th><th>Description</th><th></th></tr></thead>
                      <tbody>
                        {cats.map(c => (
                          <tr key={c.id}>
                            <td style={{ fontWeight: 600 }}>{c.name}</td>
                            <td style={{ color: 'var(--muted)' }}>{c.description || '—'}</td>
                            <td><button className="btn btn-danger btn-sm" onClick={() => deleteCat(c.id)}>Delete</button></td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </div>
              </div>
            </>
          )}
        </>
      )}

      {roleModal && (
        <div className="modal-overlay" onClick={e => e.target === e.currentTarget && setRoleModal(null)}>
          <div className="modal">
            <div className="modal-title">Change Role — {roleModal.nickName}</div>
            <div className="field">
              <label>Role</label>
              <select value={newRole} onChange={e => setNewRole(e.target.value)}>
                <option value="User">User</option>
                <option value="Admin">Admin</option>
              </select>
            </div>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setRoleModal(null)}>Cancel</button>
              <button className="btn btn-primary" onClick={changeRole}>Save</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}