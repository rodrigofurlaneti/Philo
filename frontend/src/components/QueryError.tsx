import { describeError } from "../lib/describeError";
import { Button } from "../ui";

type QueryErrorProps = { error: unknown; what: string; onRetry?: () => void };

export function QueryError({ error, what, onRetry }: QueryErrorProps) {
  return (
    <div className="empty-state" role="alert" data-testid="query-error">
      <p>
        Não foi possível carregar {what}. {describeError(error)}
      </p>
      {onRetry && (
        <Button variant="ghost" onClick={onRetry} data-testid="query-error-retry">
          Tentar de novo
        </Button>
      )}
    </div>
  );
}
