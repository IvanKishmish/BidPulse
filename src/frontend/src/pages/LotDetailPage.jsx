import { useState, useEffect, useCallback } from 'react';
import { apiFetch } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useToast } from '../context/ToastContext.jsx';
import { useCountdown } from '../hooks/useCountdown.js';

export function LotDetailPage({ lotId, setPage }) {
  const { user } = useAuth();
  const toast = useToast();
  const [lot, setLot] = useState(null);
  const [bids, setBids] = useState([]);
  const [loading, setLoading] = useState(true);
  const [bidAmt, setBidAmt] = useState('');
  const [placing, setPlacing] = useState(false);
  const timer = useCountdown(lot?.endsAt || new Date().toISOString());

  const refresh = useCallback(() => {
    if (!lotId) return;
    Promise.all([apiFetch(`/auctionlots/${lotId}`), apiFetch(`/bids/lot/${lotId}`)])
      .then(([l, b]) => { setLot(l); setBids(b); })
      .catch(() => {})
      .finally(() => setLoading(false));
  }, [lotId]);

  useEffect(() => { refresh(); }, [refresh]);

  const placeBid = async () => {
    if (!bidAmt) return;
    setPlacing(true);
    try {
      await apiFetch('/bids', {
        method: 'POST',
        body: JSON.stringify({ lotId, amount: parseFloat(bidAmt) }),
      });
      toast('Bid placed successfully!', 'success');
      setBidAmt('');
      refresh();
    } catch (e) {
      toast(e.message, 'error');
    } finally {
      setPlacing(false);
    }
  };

  const cancelLot = async () => {
    if (!confirm('Cancel this auction lot?')) return;
    try {
      await apiFetch(`/auctionlots/${lotId}`, { method: 'DELETE' });
      toast('Lot cancelled.', 'success');
      setPage('home');
    } catch (e) {
      toast(e.message, 'error');
    }
  };

  if (loading) return (
    <div className="page"><div className="loading-page"><div className="spinner spinner-dark" /></div></div>
  );
  if (!lot) return (
    <div className="page"><div className="empty-state"><h3>Lot not found</h3></div></div>
  );

  const minNext = Number(lot.currentPrice) + Number(lot.minBidStep);
  const isCreator = user?.id === lot.creatorId;

  return (
    <div className="page" style={{ maxWidth: 1100 }}>
      <button className="back-btn" onClick={() => setPage('home')}>← Back to auctions</button>

      <div className="lot-detail-grid">
        {/* Left column */}
        <div>
          <div style={{ display: 'flex', gap: 10, alignItems: 'center', flexWrap: 'wrap', marginBottom: 16 }}>
            <span className={`badge badge-${lot.status.toLowerCase()}`}>{lot.status}</span>
            <span style={{ fontSize: '.8125rem', color: 'var(--muted)', background: 'var(--surface)', padding: '3px 10px', borderRadius: 6 }}>
              {lot.categoryName}
            </span>
          </div>

          <h1 style={{ fontFamily: 'var(--font-display)', fontSize: 'clamp(1.5rem,4vw,2.25rem)', fontWeight: 800, marginBottom: 12 }}>
            {lot.title}
          </h1>
          <p className="lot-desc">{lot.description}</p>

          <div className="divider" />

          <div className="info-row">
            <div><strong>Start price</strong>${Number(lot.startPrice).toFixed(2)}</div>
            <div><strong>Min step</strong>${Number(lot.minBidStep).toFixed(2)}</div>
            <div><strong>Starts</strong>{new Date(lot.startsAt).toLocaleString()}</div>
            <div><strong>Ends</strong>{new Date(lot.endsAt).toLocaleString()}</div>
          </div>

          <div className="divider" />

          <div className="section-title" style={{ fontSize: '1.25rem', marginBottom: 16 }}>Bid History</div>
          {bids.length === 0 ? (
            <div className="empty-state" style={{ padding: '30px 0' }}>
              <div style={{ fontSize: '1.5rem', marginBottom: 8 }}>🤫</div>
              <p>No bids yet. Be the first!</p>
            </div>
          ) : (
            <div className="bid-history">
              {bids.map((b, i) => (
                <div key={b.id} className="bid-row">
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    <div style={{
                      width: 28, height: 28, borderRadius: '50%',
                      background: i === 0 ? 'var(--pulse)' : 'var(--surface)',
                      display: 'flex', alignItems: 'center', justifyContent: 'center',
                      fontSize: '.7rem', fontWeight: 700,
                      color: i === 0 ? 'white' : 'var(--muted)',
                    }}>
                      #{bids.length - i}
                    </div>
                    <span className="bid-row-amount">${Number(b.amount).toFixed(2)}</span>
                    {i === 0 && <span className="badge badge-active" style={{ fontSize: '.6rem' }}>Top</span>}
                  </div>
                  <span className="bid-row-time">{new Date(b.createdAt).toLocaleString()}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Right column – sticky bid box */}
        <div className="lot-detail-sticky">
          <div className="bid-box">
            <div className="bid-box-label">Current highest bid</div>
            <div className="bid-box-price">${Number(lot.currentPrice).toFixed(2)}</div>

            {lot.status === 'Active' && (
              <div style={{ fontFamily: 'var(--font-mono)', fontSize: '.875rem', color: 'var(--amber)', marginBottom: 20 }}>
                ⏱ {timer} remaining
              </div>
            )}

            {lot.status === 'Active' && user && !isCreator && (
              <>
                <div className="field" style={{ marginBottom: 12 }}>
                  <label style={{ color: 'rgba(255,255,255,.6)' }}>Your bid (min ${minNext.toFixed(2)})</label>
                  <input
                    type="number" step="0.01" min={minNext}
                    value={bidAmt} onChange={e => setBidAmt(e.target.value)}
                    placeholder={minNext.toFixed(2)}
                    style={{
                      background: 'rgba(255,255,255,.1)', border: '1.5px solid rgba(255,255,255,.2)',
                      color: 'white', borderRadius: 'var(--radius-sm)', padding: '10px 14px',
                      fontFamily: 'var(--font-body)', fontSize: '.9375rem', outline: 'none', width: '100%',
                    }}
                  />
                </div>
                <button className="btn btn-primary" style={{ width: '100%' }} onClick={placeBid} disabled={placing || !bidAmt}>
                  {placing ? <span className="spinner" /> : 'Place Bid'}
                </button>
              </>
            )}

            {lot.status === 'Active' && !user && (
              <p style={{ color: 'rgba(255,255,255,.5)', fontSize: '.875rem', marginTop: 8 }}>Sign in to place a bid.</p>
            )}

            {lot.status !== 'Active' && (
              <div style={{ color: 'rgba(255,255,255,.5)', fontSize: '.875rem', marginTop: 8 }}>
                This auction has {lot.status === 'Completed' ? 'ended' : 'been cancelled'}.
              </div>
            )}
          </div>

          {isCreator && lot.status === 'Active' && (
            <button className="btn btn-danger" style={{ width: '100%', marginTop: 12 }} onClick={cancelLot}>
              Cancel Auction
            </button>
          )}

          <div style={{ marginTop: 16, fontSize: '.8125rem', color: 'var(--muted)', textAlign: 'center' }}>
            Lot ID: <span style={{ fontFamily: 'var(--font-mono)' }}>{lot.id.slice(0, 8)}…</span>
          </div>
        </div>
      </div>
    </div>
  );
}