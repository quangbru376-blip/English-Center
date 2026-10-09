export type ComponentInput = {
    id: number | null;
    name: string;
    weight: string;
    kind: 'GENERAL' | 'LESSON' | 'FINAL_EXAM';
};
type Props = {
    value: readonly ComponentInput[];
    disabled: boolean;
    onChange: (next: ComponentInput[]) => void;
};
const inputStyle = 'block min-h-[44px] w-full rounded border border-ec-border bg-ec-surface p-2 text-ec-text focus-visible:ring-2 focus-visible:ring-ec-accent';
export function ProgramComponents({ value, disabled, onChange }: Props) {
    const change = (index: number, update: Partial<ComponentInput>) => { if (!disabled)
        onChange(value.map((row, i) => i === index ? { ...row, ...update } : row)); };
    return <fieldset disabled={disabled} className="space-y-3"><legend>Toàn bộ thành phần điểm</legend>
    {value.length === 0 && <p>Chưa có thành phần; có thể lưu bản nháp.</p>}
    {value.map((row, index) => <div key={row.id ?? `new-${index}`} className="grid gap-3 border border-ec-border p-3 md:grid-cols-4">
      <label>Tên thành phần {index + 1}<input className={inputStyle} value={row.name} required onChange={e => change(index, { name: e.currentTarget.value })}/></label>
      <label>Trọng số {index + 1}<input className={inputStyle} inputMode="decimal" value={row.weight} required onChange={e => change(index, { weight: e.currentTarget.value })}/></label>
      <label>Loại thành phần {index + 1}<select aria-label={`Loại thành phần ${index + 1}`} className={inputStyle} value={row.kind} onChange={e => change(index, { kind: e.currentTarget.value as ComponentInput['kind'] })}><option value="GENERAL">Chung</option><option value="LESSON">Buổi học</option><option value="FINAL_EXAM">Thi cuối khóa</option></select></label>
      <button className={inputStyle} type="button" onClick={() => { if (!disabled)
            onChange(value.filter((_, i) => i !== index)); }}>Xóa {index + 1}</button>
    </div>)}
    <button type="button" className={inputStyle} onClick={() => { if (!disabled)
        onChange([...value, { id: null, name: '', weight: '', kind: 'GENERAL' }]); }}>Thêm thành phần</button>
  </fieldset>;
}
