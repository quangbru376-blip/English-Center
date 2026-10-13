// Values are already computed/formatted by the server contract and callers.
// Never sum a page, coerce money to Number or replace no-data with zero here.
export function ReportMetrics({ items }: { items: readonly { label: string; value: string; note?: string }[] }) {
  return <dl className="ec-metrics" aria-label="Tổng quan toàn bộ tập lọc">{items.map(item=><div className="ec-metric" key={item.label}><dt>{item.label}</dt><dd>{item.value}</dd>{item.note&&<p className="ec-metric-note">{item.note}</p>}</div>)}</dl>;
}
