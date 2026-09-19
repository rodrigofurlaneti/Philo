import { useState } from "react";
import type { FormEvent, KeyboardEvent } from "react";
import { Button } from "../../ui";

type ComposerProps = {
  disabled?: boolean;
  disabledReason?: string;
  onSend: (body: string) => void;
  sending: boolean;
};

export function Composer({ disabled, disabledReason, onSend, sending }: ComposerProps) {
  const [value, setValue] = useState("");

  function submit(event?: FormEvent) {
    event?.preventDefault();
    const trimmed = value.trim();
    if (!trimmed || sending) return;
    onSend(trimmed);
    setValue("");
  }

  function onKeyDown(event: KeyboardEvent<HTMLTextAreaElement>) {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      submit();
    }
  }

  if (disabled) {
    return (
      <div className="composer" role="status">
        <p className="muted">{disabledReason ?? "Esta conversa não aceita novas mensagens."}</p>
      </div>
    );
  }

  return (
    <form className="composer" onSubmit={submit}>
      <textarea
        className="composer__input"
        placeholder="Escreva uma mensagem…"
        rows={1}
        value={value}
        onChange={(e) => setValue(e.target.value)}
        onKeyDown={onKeyDown}
        aria-label="Mensagem"
        data-testid="composer-input"
      />
      <Button type="submit" variant="primary" disabled={!value.trim() || sending} data-testid="composer-send">
        Enviar
      </Button>
    </form>
  );
}
