import { createRoot } from 'react-dom/client';
import { IdentityApp } from '../../src/app/IdentityApp';
import { HttpClient } from '../../src/lib/http';
import { AuthState } from '../../src/lib/auth';
import '../../src/styles/index.css';

// Explicit verification entry; all API requests reach the same-origin real host.
createRoot(document.getElementById('root')!).render(<IdentityApp client={new HttpClient(new AuthState())} />);
