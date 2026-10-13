import { useState } from 'react';
export type SessionFields = {
    date: string;
    startTime: string;
    endTime: string;
    room: string;
    capacity: number;
    graderId: number;
};
export function EntrySessionForm({ initial, editing, pending, graders, onSubmit, onOptions, onCancel }: {
    initial?: SessionFields;
    editing: boolean;
    pending: boolean;
    graders: readonly {
        accountId: number;
        displayName: string;
    }[];
    onSubmit: (value: SessionFields) => void;
    onOptions: (value: SessionFields) => void;
    onCancel: () => void;
}) {
    const [fields, setFields] = useState<SessionFields>(initial ?? { date: '', startTime: '', endTime: '', room: '', capacity: 1, graderId: 0 });
    const control = 'block min-h-[44px] w-full border rounded px-3 focus-visible:outline-2 focus-visible:outline-blue-700';
    return <form className="ec-work-form grid gap-4" onSubmit={event => { event.preventDefault(); if (!pending && Number.isSafeInteger(fields.capacity) && fields.capacity >= 1 && fields.capacity <= 20 && Number.isSafeInteger(fields.graderId) && fields.graderId > 0)
        onSubmit({ ...fields }); }}>
    {([['date', 'Ngày thi', 'date'], ['startTime', 'Bắt đầu', 'time'], ['endTime', 'Kết thúc', 'time'], ['room', 'Phòng', 'text']] as const).map(([key, label, type]) => <label key={key}>{label}<input className={control} required type={type} maxLength={key === 'room' ? 30 : undefined} value={fields[key]} disabled={pending} onChange={e => setFields({ ...fields, [key]: e.currentTarget.value })}/></label>)}
    {!editing && <><label>Sức chứa<input className={control} required type="number" min="1" max="20" step="1" value={fields.capacity} disabled={pending} onChange={e => setFields({ ...fields, capacity: e.currentTarget.valueAsNumber })}/></label><label>Giáo viên chấm<select aria-label="Giáo viên chấm" className={control} required value={fields.graderId || ''} disabled={pending} onChange={e => setFields({ ...fields, graderId: Number(e.currentTarget.value) })}><option value="">Chọn giáo viên</option>{graders.map(g => <option key={g.accountId} value={g.accountId}>{g.displayName}</option>)}</select></label></>}
    <button className={control} type="button" disabled={pending} onClick={() => onOptions({ ...fields })}>Xem giáo viên phù hợp</button><button className={control} type="submit" disabled={pending}>Lưu</button><button className={control} type="button" disabled={pending} onClick={onCancel}>Hủy</button>
  </form>;
}
