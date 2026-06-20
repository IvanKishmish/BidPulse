import { useCountdown } from '../hooks/useCountdown.js';

export function LotCard({ lot, onClick }) {
  const timer = useCountdown(lot.endsAt);

  return (
    <div className="card lot-card" onClick={onClick}>
      <div className="lot-card-header">
        <div className="lot-card-category">{lot.categoryName}</div>
        <div className="lot-card-title">{lot.title}</div>
        <span className={`badge badge-${lot.status.toLowerCase()}`}>{lot.status}</span>
      </div>
      <div className="lot-card-body">
        <div className="lot-price-row">
          <div>
            <div className="lot-price-label">Current Price</div>
            <div className="lot-price">${Number(lot.currentPrice).toFixed(2)}</div>
          </div>
          <div style={{ textAlign: 'right' }}>
            <div className="lot-price-label">Started at</div>
            <div style={{ fontFamily: 'var(--font-mono)', fontSize: '.9rem', fontWeight: 600, color: 'var(--muted)' }}>
              ${Number(lot.startPrice).toFixed(2)}
            </div>
          </div>
        </div>
        <div className="lot-meta">
          {lot.status === 'Active' && <span className="lot-timer">⏱ {timer}</span>}
          <span style={{ fontSize: '.8125rem', color: 'var(--muted)' }}>
            Step: ${Number(lot.minBidStep).toFixed(2)}
          </span>
        </div>
      </div>
    </div>
  );
}