import { useState } from 'react';
import { AuthProvider, useAuth } from './context/AuthContext.jsx';
import { ToastProvider } from './context/ToastContext.jsx';
import { Nav } from './components/Nav.jsx';
import { HomePage } from './pages/HomePage.jsx';
import { LotDetailPage } from './pages/LotDetailPage.jsx';
import { CreateLotPage } from './pages/CreateLotPage.jsx';
import { WalletPage } from './pages/WalletPage.jsx';
import { ProfilePage } from './pages/ProfilePage.jsx';
import { AdminPage } from './pages/AdminPage.jsx';
import { LoginPage, RegisterPage } from './pages/AuthPages.jsx';

function Router() {
  const { user, loading } = useAuth();
  const [page, setPage] = useState('home');
  const [activeLot, setActiveLot] = useState(null);

  if (loading) {
    return (
      <div className="loading-page" style={{ height: '100vh' }}>
        <div className="spinner spinner-dark" />
      </div>
    );
  }

  const openLot = (lot) => { setActiveLot(lot); setPage('lot'); };

  const renderPage = () => {
    switch (page) {
      case 'home':    return <HomePage setPage={setPage} setActiveLot={openLot} />;
      case 'lot':     return <LotDetailPage lotId={activeLot?.id} setPage={setPage} />;
      case 'create':  return user ? <CreateLotPage setPage={setPage} /> : <LoginPage setPage={setPage} />;
      case 'wallet':  return user ? <WalletPage /> : <LoginPage setPage={setPage} />;
      case 'profile': return user ? <ProfilePage /> : <LoginPage setPage={setPage} />;
      case 'admin':   return user?.role === 'Admin' ? <AdminPage /> : <HomePage setPage={setPage} setActiveLot={openLot} />;
      case 'login':   return <LoginPage setPage={setPage} />;
      case 'register':return <RegisterPage setPage={setPage} />;
      default:        return <HomePage setPage={setPage} setActiveLot={openLot} />;
    }
  };

  return (
    <>
      <Nav page={page} setPage={setPage} />
      <main style={{ flex: 1 }}>
        {renderPage()}
      </main>
      <footer style={{ borderTop: '1px solid var(--border)', padding: '24px', textAlign: 'center', color: 'var(--muted)', fontSize: '.8125rem' }}>
        © {new Date().getFullYear()} BidPulse — Real-time Auction Platform
      </footer>
    </>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <ToastProvider>
        <Router />
      </ToastProvider>
    </AuthProvider>
  );
}