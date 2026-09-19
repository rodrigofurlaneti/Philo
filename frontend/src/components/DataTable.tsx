import type { ReactNode } from "react";

export type DataTableColumn<T> = {
  header: string;
  render: (row: T) => ReactNode;
  align?: "left" | "right";
};

type DataTableProps<T> = {
  columns: DataTableColumn<T>[];
  rows: T[];
  rowKey: (row: T) => string | number;
  testId: string;
  emptyMessage?: string;
};

/** AGENTS.md §8: passar data-testid="x-table" faz as linhas virarem "x-table-row". */
export function DataTable<T>({ columns, rows, rowKey, testId, emptyMessage }: DataTableProps<T>) {
  if (rows.length === 0) {
    return <p className="muted" data-testid={`${testId}-empty`}>{emptyMessage ?? "Nada por aqui ainda."}</p>;
  }

  return (
    <div style={{ overflowX: "auto" }}>
      <table data-testid={testId} style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr>
            {columns.map((col) => (
              <th
                key={col.header}
                style={{
                  textAlign: col.align ?? "left",
                  padding: "10px 12px",
                  borderBottom: "1px solid var(--line)",
                  color: "var(--ink-dim)",
                  fontSize: 12,
                  fontWeight: 600,
                }}
              >
                {col.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={rowKey(row)} data-testid={`${testId}-row`}>
              {columns.map((col) => (
                <td
                  key={col.header}
                  style={{ textAlign: col.align ?? "left", padding: "10px 12px", borderBottom: "1px solid var(--line-soft)" }}
                >
                  {col.render(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
