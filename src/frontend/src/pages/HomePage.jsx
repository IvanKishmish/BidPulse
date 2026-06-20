import { useState, useEffect } from 'react';
import { apiFetch } from '../api/client.js';
import { LotCard } from '../components/LotCard.jsx';

export function HomePage({ setPage, setActiveLot }) {
  const [lots, setLots] = useState([]);
  const [categories, setCategories] = useState([]);
  const [loading, setLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [catFilter, setCatFilter] = useState('all');
  const [statusFilter, setStatusFilter] = useState('all');

  useEffect(() => {
    Promise.all([apiFetch('/auctionlots'), apiFetch('/categories')])
      .then(([l, c]) => { setLots(l); setCategories(c); })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, []);

  const filtered = lots.filter(lot => {
    if (search && !lot.title.toLowerCase().includes(search.toLowerCase())) return false;
    if (catFilter !== 'all' && lot.categoryId !== catFilter) return false;
    if (statusFilter !== 'all' && lot.status !== statusFilter) return false;
    return true;
  });

  const stats = {
    total: lots.length,
    active: lots.filter(x => x.status === 'Active').length,
    cats: categories.length,
  };

  const open = (lot) => { setActiveLot(lot); setPage('lot'); };

  return (
    <>
      <div className="hero">
        <div className="hero-tag">
          <div style={{ width: 6, height: 6, borderRadius: '50%', background: 'currentColor' }} />
          Live Auctions
        </div>
        <h1>Bid Smart,<br /><span>Win Big.</span></h1>
        <p>Discover rare items, set your price, and compete in real-time auctions — all with instant fund management.</p>
        <div className="hero-actions">
          <button className="btn btn-primary btn-lg" onClick={() => document.getElementById('lots-section').scrollIntoView({ behavior: 'smooth' })}>
            Browse Lots
          </button>
        </div>
        <div className="hero-stats">
          <div className="hero-stat">
            <div className="hero-stat-num">{stats.total}</div>
            <div className="hero-stat-label">Total Lots</div>
          </div>
          <div className="hero-stat">
            <div className="hero-stat-num">{stats.active}</div>
            <div className="hero-stat-label">Active Now</div>
          </div>
          <div className="hero-stat">
            <div className="hero-stat-num">{stats.cats}</div>
            <div className="hero-stat-label">Categories</div>
          </div>
        </div>
      </div>

      <div className="page" id="lots-section">
        <div className="section-header">
          <div>
            <div className="section-title">All Auctions</div>
            <div className="section-sub">{filtered.length} lots found</div>
          </div>
          <div className="search-bar">
            <span style={{ color: 'var(--muted)' }}>🔍</span>
            <input placeholder="Search lots…" value={search} onChange={e => setSearch(e.target.value)} />
          </div>
        </div>

        <div className="filters">
          <button className={`filter-chip ${statusFilter === 'all' ? 'active' : ''}`} onClick={() => setStatusFilter('all')}>All Status</button>
          <button className={`filter-chip ${statusFilter === 'Active' ? 'active' : ''}`} onClick={() => setStatusFilter('Active')}>Active</button>
          <button className={`filter-chip ${statusFilter === 'Completed' ? 'active' : ''}`} onClick={() => setStatusFilter('Completed')}>Completed</button>
          <button className={`filter-chip ${statusFilter === 'Cancelled' ? 'active' : ''}`} onClick={() => setStatusFilter('Cancelled')}>Cancelled</button>
          <div style={{ width: 1, background: 'var(--border)', margin: '0 4px' }} />
          <button className={`filter-chip ${catFilter === 'all' ? 'active' : ''}`} onClick={() => setCatFilter('all')}>All Categories</button>
          {categories.map(c => (
            <button key={c.id} className={`filter-chip ${catFilter === c.id ? 'active' : ''}`} onClick={() => setCatFilter(c.id)}>
              {c.name}
            </button>
          ))}
        </div>

        {loading ? (
          <div className="loading-page"><div className="spinner spinner-dark" /></div>
        ) : filtered.length === 0 ? (
          <div className="empty-state">
            <div className="empty-state-icon">🔍</div>
            <h3>No lots found</h3>
            <p>Try changing the filters or search term.</p>
          </div>
        ) : (
          <div className="lot-grid">
            {filtered.map(lot => <LotCard key={lot.id} lot={lot} onClick={() => open(lot)} />)}
          </div>
        )}
      </div>
    </>
  );
}