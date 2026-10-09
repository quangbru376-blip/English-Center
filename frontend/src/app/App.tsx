import { useState } from 'react';
import { IdentityApp } from './IdentityApp';
import { AuthState } from '../lib/auth';
import { HttpClient } from '../lib/http';

export default function App() {
  const [client] = useState(() => new HttpClient(new AuthState()));
  return <IdentityApp client={client} />;
}
