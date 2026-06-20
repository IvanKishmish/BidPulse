import { useAuth } from '../context/AuthContext.jsx';

export function Nav({ page, setPage }) {
  const { user, logout } = useAuth();

  return (
    <nav className="nav">
      <div className="nav-inner">
        <a className="nav-logo" onClick={() => setPage('home')}>
          <div className="nav-logo-dot" />
          BidPulse
        </a>
        <div className="nav-links">
          <button className={`nav-link ${page === 'home' ? 'active' : ''}`} onClick={() => setPage('home')}>
            Auctions
          </button>
          {user && (
            <>
              <button className={`nav-link ${page === 'create' ? 'active' : ''}`} onClick={() => setPage('create')}>
                Create Lot
              </button>
              <button className={`nav-link ${page === 'wallet' ? 'active' : ''}`} onClick={() => setPage('wallet')}>
                Wallet
              </button>
              {user.role === 'Admin' && (
                <button className={`nav-link ${page === 'admin' ? 'active' : ''}`} onClick={() => setPage('admin')}>
                  Admin
                </button>
              )}
              {user.balance !== undefined && (
                <span className="nav-balance">${Number(user.balance).toFixed(2)}</span>
              )}
              <div className="nav-avatar" title={user.nickName} onClick={() => setPage('profile')}>
                {user.nickName?.[0]?.toUpperCase() || 'U'}
              </div>
              <button className="btn btn-ghost btn-sm" style={{ marginLeft: 4 }} onClick={logout}>
                Sign out
              </button>
            </>
          )}
          {!user && (
            <>
              <button className={`nav-link ${page === 'login' ? 'active' : ''}`} onClick={() => setPage('login')}>
                Sign in
              </button>
              <button className="btn btn-primary btn-sm" style={{ marginLeft: 4 }} onClick={() => setPage('register')}>
                Sign up
              </button>
            </>
          )}
        </div>
      </div>
    </nav>
  );
}