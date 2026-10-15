type Action = { action: string; label: string };

// Show server reasons only for actions present in this group, once per reason.
export function StudentActionReasons({ actions, allowedActions, reasons }: {
  actions: readonly Action[];
  allowedActions: readonly string[];
  reasons: Readonly<Record<string, string>>;
}) {
  const groups = new Map<string, string[]>();
  for (const { action, label } of actions) {
    if (allowedActions.includes(action) || !reasons[action]) continue;
    const reason = reasons[action];
    groups.set(reason, [...(groups.get(reason) ?? []), label]);
  }
  if (!groups.size) return null;
  return <ul className="student-reasons" aria-label="Lý do thao tác chưa khả dụng">
    {[...groups].map(([reason, labels]) => <li key={reason}><span className="font-medium">{labels.join(', ')}:</span> {reason}</li>)}
  </ul>;
}
