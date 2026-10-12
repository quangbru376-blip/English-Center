export type PublishedComponent = {
  componentId: number;
  name: string;
  kind: 'GENERAL' | 'LESSON' | 'FINAL_EXAM';
  weight: string;
  score: string;
  scheduleLabel: string | null;
};

export type StudentGradeResultsProps = {
  classCode: string;
  programName: string;
  finalizedLabel: string;
  finalScore: string;
  passed: boolean;
  teacherComment: string | null;
  components: ReadonlyArray<PublishedComponent>;
};

const KIND_LABELS: Record<PublishedComponent['kind'], string> = {
  GENERAL: 'Chung',
  LESSON: 'Buổi học',
  FINAL_EXAM: 'Thi cuối khóa',
};

export function StudentGradeResults(props: StudentGradeResultsProps) {
  const resultText = props.passed ? 'Đạt' : 'Chưa đạt';

  return (
    <section className="min-w-0 max-w-full break-words rounded-lg border border-[var(--ec-border)] bg-[var(--ec-surface)] p-4 text-[var(--ec-text)] font-sans sm:p-6">
      <header className="border-b border-[var(--ec-border)] pb-4">
        <h2 className="text-lg font-semibold sm:text-xl">Kết quả học tập</h2>
        <dl className="mt-3 grid grid-cols-1 gap-2 text-sm sm:grid-cols-3">
          <div>
            <dt className="text-xs opacity-75">Mã lớp</dt>
            <dd className="font-medium">{props.classCode}</dd>
          </div>
          <div>
            <dt className="text-xs opacity-75">Chương trình học</dt>
            <dd className="font-medium">{props.programName}</dd>
          </div>
          <div>
            <dt className="text-xs opacity-75">Thời điểm công bố</dt>
            <dd className="font-medium">{props.finalizedLabel}</dd>
          </div>
        </dl>
      </header>

      <div className="my-4 rounded-md border border-[var(--ec-border)] p-4">
        <dl className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div>
            <dt className="text-xs opacity-75">Điểm tổng kết</dt>
            <dd className="text-2xl font-bold">{props.finalScore}</dd>
          </div>
          <div>
            <dt className="text-xs opacity-75">Kết quả</dt>
            <dd className="text-2xl font-bold">{resultText}</dd>
          </div>
        </dl>
      </div>

      <section className="mb-4 rounded-md border border-[var(--ec-border)] p-3">
        <h3 className="text-xs font-medium opacity-75">Nhận xét của giáo viên</h3>
        <p className="mt-1 text-sm">{props.teacherComment ?? 'Chưa có nhận xét'}</p>
      </section>

      <section className="space-y-3">
        <h3 className="text-sm font-semibold">Thành phần điểm</h3>
        <div className="space-y-2">
          {props.components.length === 0 ? (
            <p className="text-xs opacity-75">Chưa có thành phần điểm</p>
          ) : (
            props.components.map((component) => (
              <article
                key={component.componentId}
                className="min-w-0 break-words rounded-md border border-[var(--ec-border)] p-3 text-sm"
              >
                <div className="flex flex-wrap items-center justify-between gap-2">
                  <span className="font-medium">{component.name}</span>
                  <span className="rounded border border-[var(--ec-border)] px-2 py-0.5 text-xs">
                    {KIND_LABELS[component.kind] ?? component.kind}
                  </span>
                </div>
                {component.scheduleLabel ? (
                  <p className="mt-1 text-xs opacity-75">{component.scheduleLabel}</p>
                ) : null}
                <dl className="mt-2 grid grid-cols-2 gap-2 text-xs">
                  <div>
                    <dt className="opacity-75">Trọng số</dt>
                    <dd className="font-medium">
                      {component.weight}%
                    </dd>
                  </div>
                  <div>
                    <dt className="opacity-75">Điểm</dt>
                    <dd className="font-medium">{component.score}</dd>
                  </div>
                </dl>
              </article>
            ))
          )}
        </div>
      </section>
    </section>
  );
}
