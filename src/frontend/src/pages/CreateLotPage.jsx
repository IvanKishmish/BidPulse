import { useState, useEffect } from 'react';
import { apiFetch } from '../api/client.js';
import { useToast } from '../context/ToastContext.jsx';

export function CreateLotPage({ setPage }) {
  const toast = useToast();
  const [categories, setCategories] = useState([]);
  const [form, setForm] = useState({
    title: '', description: '', startPrice: '', minBidStep: '',
    startsAt: '', endsAt: '', categoryId: '',
  });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    apiFetch('/categories').then(setCategories).catch(() => {});
  }, []);

  const set = k => e => setForm(f => ({ ...f, [k]: e.target.value }));

  const submit = async () => {
    setLoading(true);
    try {
      await apiFetch('/auctionlots', {
        method: 'POST',
        body: JSON.stringify({
          title: form.title,
          description: form.description,
          startPrice: parseFloat(form.startPrice),
          minBidStep: parseFloat(form.minBidStep),
          startsAt: new Date(form.startsAt).toISOString(),
          endsAt: new Date(form.endsAt).toISOString(),
          categoryId: form.categoryId,
        }),
      });
      toast('Auction lot created!', 'success');
      setPage('home');
    } catch (e) {
      toast(e.message, 'error');
    } finally {
      setLoading(false);
    }
  };

  const isValid = form.title && form.categoryId && form.startPrice && form.minBidStep && form.startsAt && form.endsAt;

  return (
    <div className="page page-md">
      <button className="back-btn" onClick={() => setPage('home')}>← Back</button>
      <div className="section-title">Create Auction Lot</div>
      <div className="section-sub">Fill in the details for your new auction.</div>

      <div className="card" style={{ marginTop: 4 }}>
        <div className="card-body" style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <div className="field">
            <label>Title</label>
            <input placeholder="e.g. Vintage Rolex Submariner" value={form.title} onChange={set('title')} />
          </div>

          <div className="field">
            <label>Description</label>
            <textarea placeholder="Describe your item in detail…" value={form.description} onChange={set('description')} />
          </div>

          <div className="form-grid">
            <div className="field">
              <label>Start Price ($)</label>
              <input type="number" step="0.01" min="0" placeholder="0.00" value={form.startPrice} onChange={set('startPrice')} />
            </div>
            <div className="field">
              <label>Min Bid Step ($)</label>
              <input type="number" step="0.01" min="0" placeholder="1.00" value={form.minBidStep} onChange={set('minBidStep')} />
            </div>
            <div className="field">
              <label>Starts At</label>
              <input type="datetime-local" value={form.startsAt} onChange={set('startsAt')} />
            </div>
            <div className="field">
              <label>Ends At</label>
              <input type="datetime-local" value={form.endsAt} onChange={set('endsAt')} />
            </div>
          </div>

          <div className="field">
            <label>Category</label>
            <select value={form.categoryId} onChange={set('categoryId')}>
              <option value="">— Select category —</option>
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>

          <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end', marginTop: 8 }}>
            <button className="btn btn-secondary" onClick={() => setPage('home')}>Cancel</button>
            <button className="btn btn-primary" onClick={submit} disabled={loading || !isValid}>
              {loading ? <span className="spinner" /> : 'Create Lot'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}