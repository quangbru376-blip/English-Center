import { useState } from 'react';
import { createRoot } from 'react-dom/client';
import { IdentityShell } from '../src/app/IdentityShell';
import { IdentityApp } from '../src/app/IdentityApp';
import { HttpClient } from '../src/lib/http';
import { AuthState } from '../src/lib/auth';
import { SignupPage } from '../src/features/identity/pages/SignupPage';
import { Button } from '../src/components/ui/Button';
import '../src/styles/index.css';

// Synthetic fixture; browser tests intercept API requests. No production entry.
const client = new HttpClient(new AuthState());
function Fixture() {
  const [login, setLogin] = useState(false);
  return <IdentityShell>{login ? <h1>Đăng nhập — điểm chuyển sau đăng ký</h1> :
    <SignupPage client={client} onCreated={() => setLogin(true)} onLogin={() => setLogin(true)} />}
    <Button variant="secondary" onClick={() => client.auth.clear()}>Đổi ngữ cảnh kiểm chứng</Button>
  </IdentityShell>;
}
createRoot(document.getElementById('root')!).render(new URLSearchParams(location.search).has('session')
  ? <IdentityApp client={client} initialPath="/login" /> : <Fixture />);
