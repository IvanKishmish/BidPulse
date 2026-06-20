import { useState, useEffect } from 'react';

export function useCountdown(endsAt) {
  const [rem, setRem] = useState('');

  useEffect(() => {
    const tick = () => {
      const diff = new Date(endsAt) - Date.now();
      if (diff <= 0) { setRem('Ended'); return; }
      const d = Math.floor(diff / 86400000);
      const h = Math.floor((diff % 86400000) / 3600000);
      const m = Math.floor((diff % 3600000) / 60000);
      const s = Math.floor((diff % 60000) / 1000);
      if (d > 0) setRem(`${d}d ${h}h`);
      else setRem(`${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`);
    };
    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, [endsAt]);

  return rem;
}