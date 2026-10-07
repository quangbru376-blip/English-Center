const signal = 'IDENTITY_CHANGED';
export type SessionSignalPort = Readonly<{ send: () => void; close: () => void }>;
type Channel = Pick<BroadcastChannel, 'postMessage' | 'close' | 'addEventListener' | 'removeEventListener'>;
export function createSessionSignals(onChanged: () => void, factory?: () => Channel): SessionSignalPort {
  let channel: Channel | null = null;
  try { channel = factory ? factory() : typeof BroadcastChannel === 'undefined' ? null : new BroadcastChannel('ec-identity-v1'); }
  catch { /* Unsupported channel uses caller focus/visibility proof; never storage. */ }
  let closed = false;
  const receive = (event: MessageEvent<unknown>) => { if (!closed && event.data === signal) onChanged(); };
  channel?.addEventListener('message', receive as EventListener);
  return Object.freeze({
    send: () => { if (!closed) { try { channel?.postMessage(signal); } catch { /* Focus/visibility still revalidates without storage. */ } } },
    close: () => { if (closed) return; closed = true; channel?.removeEventListener('message', receive as EventListener); channel?.close(); },
  });
}
