import { useState, useEffect } from 'react';
import { apiFetch } from '../api/client.js';
import { useAuth } from '../context/AuthContext.jsx';
import { useToast } from '../context/ToastContext.jsx';

const TX_PLUS = ['Deposit', 'Refund'];
const TX_COLOR = type => TX_PLUS.includes(type) ? 'var(--green)' : 'var(--red)';
const TX_SIGN  = type => TX_PLUS.includes(type) ? '+' : '-';

export function WalletPage() {
  const { user, refreshUser } = useAuth();
  const toast = useToast();
  const [txns, setTxns] = useState([]);
  const [loading, setLoading] = useState(true);
  const [depositAmt, setDepositAmt] = useState('');
  const [withdrawAmt, setWithdrawAmt] = useState('');
  const [working, setWorking] = useState(false);

  const userId = user?.id;

  const loadTxns = () => {
    apiFetch(`/users/${userId}/wallet`).then(setTxns).catch(() => {});
  };

  useEffect(() => {
    if (!userId) return;
    apiFetch(`/users/${userId}/wallet`)
      .then(setTxns).catch(() => {}).finally(() => setLoading(false));
  }, [userId]);

  const doDeposit = async () => {
    if (!depositAmt) return;
    setWorking(true);
    try {
      await apiFetch(`/users/${userId}/wallet/deposit`, {
        method: 'POST',
        body: JSON.stringify({ amount: parseFloat(depositAmt) }),
      });
      toast(`Deposited $${depositAmt}`, 'success');
      setDepositAmt('');
      refreshUser();
      loadTxns();
    } catch (e) { toast(e.message, 'error'); }
    finally { setWorking(false); }
  };

  const doWithdraw = async () => {
    if (!withdrawAmt) return;
    setWorking(true);
    try {
      await apiFetch(`/users/${userId}/wallet/withdraw`, {
        method: 'POST',
        body: JSON.stringify({ amount: parseFloat(withdrawAmt) }),
      });
      toast(`Withdrew $${withdrawAmt}`, 'success');
      setWithdrawAmt('');
      refreshUser();
      loadTxns();
    } catch (e) { toast(e.message, 'error'); }
    finally { setWorking(false); }
  };

  return (
    <div className="page page-md">
      <div className="section-title">Wallet</div>
      <div className="section-sub">Manage your funds and view transaction history.</div>

      <div className="wallet-row">
        <div className="wallet-stat">
          <div className="wallet-stat-label">Balance</div>
          <div className="wallet-stat-val" style={{ color: 'var(--pulse)' }}>${Number(user?.balance || 0).toFixed(2)}</div>
        </div>
        <div className="wallet-stat">
          <div className="wallet-stat-label">Transactions</div>
          <div className="wallet-stat-val">{txns.length}</div>
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 16, marginBottom: 28 }}>
        <div className="card">
          <div className="card-body">
            <div style={{ fontWeight: 700, marginBottom: 12, color: 'var(--green)' }}>↑ Deposit</div>
            <div className="field">
              <input type="number" step="0.01" min="0" placeholder="Amount ($)"
                value={depositAmt} onChange={e => setDepositAmt(e.target.value)} />
            </div>
            <button className="btn btn-primary" style={{ width: '100%', marginTop: 10, background: 'var(--green)' }}
              onClick={doDeposit} disabled={working || !depositAmt}>
              {working ? <span className="spinner" /> : 'Deposit Funds'}
            </button>
          </div>
        </div>

        <div className="card">
          <div className="card-body">
            <div style={{ fontWeight: 700, marginBottom: 12, color: 'var(--red)' }}>↓ Withdraw</div>
            <div className="field">
              <input type="number" step="0.01" min="0" placeholder="Amount ($)"
                value={withdrawAmt} onChange={e => setWithdrawAmt(e.target.value)} />
            </div>
            <button className="btn btn-danger" style={{ width: '100%', marginTop: 10 }}
              onClick={doWithdraw} disabled={working || !withdrawAmt}>
              {working ? <span className="spinner" /> : 'Withdraw Funds'}
            </button>
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-body">
          <div style={{ fontWeight: 700, marginBottom: 16 }}>Transaction History</div>
          {loading ? (
            <div className="loading-page" style={{ height: 120 }}><div className="spinner spinner-dark" /></div>
          ) : txns.length === 0 ? (
            <div className="empty-state" style={{ padding: '30px 0' }}><p>No transactions yet.</p></div>
          ) : (
            <div className="table-wrap">
              <table>
                <thead><tr><th>Type</th><th>Amount</th><th>Date</th><th>Lot</th></tr></thead>
                <tbody>
                  {[...txns].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt)).map(t => (
                    <tr key={t.id}>
                      <td>
                        <span className="badge" style={{ background: 'var(--surface)', color: 'var(--ink)', border: '1px solid var(--border)' }}>
                          {t.type}
                        </span>
                      </td>
                      <td>
                        <span style={{ fontFamily: 'var(--font-mono)', fontWeight: 700, color: TX_COLOR(t.type) }}>
                          {TX_SIGN(t.type)}${Number(t.amount).toFixed(2)}
                        </span>
                      </td>
                      <td style={{ color: 'var(--muted)', fontSize: '.8125rem' }}>{new Date(t.createdAt).toLocaleString()}</td>
                      <td style={{ color: 'var(--muted)', fontSize: '.8125rem', fontFamily: 'var(--font-mono)' }}>
                        {t.lotId ? t.lotId.slice(0, 8) + '…' : '—'}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}