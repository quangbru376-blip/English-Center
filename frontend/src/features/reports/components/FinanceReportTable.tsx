import {moneyText} from '../reportWire';
export type FinanceMetrics = {
  revenueAmount: string;
  billedAmount: string;
  currentDebtAmount: string;
  overdueDebtAmount: string;
};

export type FinanceGroup = {
  course: {
    id: number;
    code: string;
    name: string;
  };
  metrics: FinanceMetrics;
};

export function FinanceReportTable({
  items,
  totals,
  overdueOnly,
}: {
  items: readonly FinanceGroup[];
  totals: FinanceMetrics;
  overdueOnly: boolean;
}) {
  const rows = items;

  return (
    <div style={{ overflowX: 'auto' }} className="overflow-x-auto">
      <table className="min-w-full divide-y divide-[var(--ec-border)] text-left text-sm">
        <caption className="p-2 text-left font-semibold text-[var(--ec-text)]">
          Báo cáo tài chính{overdueOnly ? ' — Công nợ hiện tại — chỉ quá hạn' : ''}
        </caption>
        <thead className="bg-[var(--ec-background)]">
          <tr>
            <th scope="col" className="px-3 py-2 text-left font-medium text-[var(--ec-muted)]">
              Mã khóa
            </th>
            <th scope="col" className="px-3 py-2 text-left font-medium text-[var(--ec-muted)]">
              Tên khóa
            </th>
            <th scope="col" className="px-3 py-2 text-right font-medium text-[var(--ec-muted)]">
              Doanh thu trong kỳ (VND)
            </th>
            <th scope="col" className="px-3 py-2 text-right font-medium text-[var(--ec-muted)]">
              Phải thu trong kỳ (VND)
            </th>
            <th scope="col" className="px-3 py-2 text-right font-medium text-[var(--ec-muted)]">
              Công nợ hiện tại (VND)
            </th>
            <th scope="col" className="px-3 py-2 text-right font-medium text-[var(--ec-muted)]">
              Công nợ quá hạn (VND)
            </th>
          </tr>
        </thead>
        <tbody className="divide-y divide-[var(--ec-border)] bg-[var(--ec-surface)]">
          {rows.length === 0 ? (
            <tr>
              <td colSpan={6} className="px-3 py-4 text-center text-[var(--ec-muted)]">
                Chưa có dữ liệu
              </td>
            </tr>
          ) : (
            rows.map((item) => (
              <tr key={item.course.id}>
                <td className="px-3 py-2 text-left text-[var(--ec-text)]">{item.course.code}</td>
                <td className="px-3 py-2 text-left text-[var(--ec-text)]">{item.course.name}</td>
                <td className="px-3 py-2 text-right text-[var(--ec-text)]">{moneyText(item.metrics.revenueAmount)}</td>
                <td className="px-3 py-2 text-right text-[var(--ec-text)]">{moneyText(item.metrics.billedAmount)}</td>
                <td className="px-3 py-2 text-right text-[var(--ec-text)]">{moneyText(item.metrics.currentDebtAmount)}</td>
                <td className="px-3 py-2 text-right text-[var(--ec-text)]">{moneyText(item.metrics.overdueDebtAmount)}</td>
              </tr>
            ))
          )}
        </tbody>
        <tfoot className="border-t-2 border-[var(--ec-border)] font-semibold text-[var(--ec-text)]">
          <tr>
            <td className="px-3 py-2"></td>
            <td className="px-3 py-2 text-left">Tổng toàn bộ tập lọc</td>
            <td className="px-3 py-2 text-right">{moneyText(totals.revenueAmount)}</td>
            <td className="px-3 py-2 text-right">{moneyText(totals.billedAmount)}</td>
            <td className="px-3 py-2 text-right">{moneyText(totals.currentDebtAmount)}</td>
            <td className="px-3 py-2 text-right">{moneyText(totals.overdueDebtAmount)}</td>
          </tr>
        </tfoot>
      </table>
    </div>
  );
}
