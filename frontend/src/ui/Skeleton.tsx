type SkeletonProps = { width?: string | number; height?: string | number; circle?: boolean };

export function Skeleton({ width = "100%", height = 16, circle = false }: SkeletonProps) {
  return (
    <div
      className="skeleton"
      role="status"
      aria-label="Carregando"
      style={{ width, height, borderRadius: circle ? 999 : undefined }}
    />
  );
}
