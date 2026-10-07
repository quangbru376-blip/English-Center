export type RequestTicket = Readonly<{
  signal: AbortSignal;
  isCurrent: () => boolean;
  finish: () => void;
}>;

interface SlotEntry {
  readonly epoch: number;
  readonly controller: AbortController;
}

export class RequestScope {
  #epoch = 0;
  readonly #slots = new Map<string, SlotEntry>();
  readonly #cleanups = new Set<() => void>();

  begin(slot: string): RequestTicket {
    if (typeof slot !== 'string' || slot.trim().length === 0) {
      throw new Error('Slot must be a non-empty string.');
    }

    const previous = this.#slots.get(slot);
    if (previous) {
      try {
        previous.controller.abort();
      } catch {
        // Suppress abort errors on superseded tickets.
      }
    }

    const controller = new AbortController();
    const capturedEpoch = this.#epoch;
    let finished = false;

    const entry: SlotEntry = {
      epoch: capturedEpoch,
      controller,
    };

    this.#slots.set(slot, entry);

    const ticket: RequestTicket = Object.freeze({
      signal: controller.signal,
      isCurrent: (): boolean => {
        return (
          !finished &&
          !controller.signal.aborted &&
          capturedEpoch === this.#epoch &&
          this.#slots.get(slot) === entry
        );
      },
      finish: (): void => {
        if (finished) {
          return;
        }
        finished = true;
        if (this.#slots.get(slot) === entry) {
          this.#slots.delete(slot);
        }
      },
    });

    return ticket;
  }

  invalidate(): void {
    this.#epoch += 1;

    const outstandingEntries = Array.from(this.#slots.values());
    this.#slots.clear();

    for (const entry of outstandingEntries) {
      try {
        entry.controller.abort();
      } catch {
        // Invalidation failures cannot prevent other tickets from aborting.
      }
    }

    const snapshot = Array.from(this.#cleanups);
    for (const cleanup of snapshot) {
      try {
        cleanup();
      } catch {
        // Cleanup failures cannot prevent other cleanups or epoch invalidation.
      }
    }
  }

  registerCleanup(cleanup: () => void): () => void {
    if (typeof cleanup !== 'function') {
      throw new TypeError('Cleanup callback must be a function.');
    }

    this.#cleanups.add(cleanup);
    let unregistered = false;

    return (): void => {
      if (unregistered) {
        return;
      }
      unregistered = true;
      this.#cleanups.delete(cleanup);
    };
  }
}
