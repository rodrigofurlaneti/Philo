import { forwardRef, useId } from "react";
import type { InputHTMLAttributes, ReactNode, SelectHTMLAttributes, TextareaHTMLAttributes } from "react";

type FieldChromeProps = {
  label: string;
  hint?: string;
  error?: string;
  children: (ids: { inputId: string; describedBy: string | undefined }) => ReactNode;
};

/**
 * `Field` é a referência de acessibilidade do design system (AGENTS.md §7):
 * useId(), htmlFor, aria-invalid, aria-describedby ligando dica e erro.
 */
function Field({ label, hint, error, children }: FieldChromeProps) {
  const inputId = useId();
  const hintId = hint ? `${inputId}-hint` : undefined;
  const errorId = error ? `${inputId}-error` : undefined;
  const describedBy = [hintId, errorId].filter(Boolean).join(" ") || undefined;

  return (
    <div className="field">
      <label className="field__label" htmlFor={inputId}>
        {label}
      </label>
      {children({ inputId, describedBy })}
      {hint && !error && (
        <span className="field__hint" id={hintId}>
          {hint}
        </span>
      )}
      {error && (
        <span className="field__error" id={errorId} role="alert">
          {error}
        </span>
      )}
    </div>
  );
}

type TextFieldProps = InputHTMLAttributes<HTMLInputElement> & {
  label: string;
  hint?: string;
  error?: string;
};

/** Todo campo que recebe {...register(...)} precisa de forwardRef (AGENTS.md §5). */
export const TextField = forwardRef<HTMLInputElement, TextFieldProps>(
  ({ label, hint, error, ...props }, ref) => (
    <Field label={label} hint={hint} error={error}>
      {({ inputId, describedBy }) => (
        <input
          ref={ref}
          id={inputId}
          className="field__control"
          aria-invalid={Boolean(error)}
          aria-describedby={describedBy}
          {...props}
        />
      )}
    </Field>
  ),
);
TextField.displayName = "TextField";

type TextAreaFieldProps = TextareaHTMLAttributes<HTMLTextAreaElement> & {
  label: string;
  hint?: string;
  error?: string;
};

export const TextAreaField = forwardRef<HTMLTextAreaElement, TextAreaFieldProps>(
  ({ label, hint, error, ...props }, ref) => (
    <Field label={label} hint={hint} error={error}>
      {({ inputId, describedBy }) => (
        <textarea
          ref={ref}
          id={inputId}
          className="field__control"
          aria-invalid={Boolean(error)}
          aria-describedby={describedBy}
          {...props}
        />
      )}
    </Field>
  ),
);
TextAreaField.displayName = "TextAreaField";

type SelectFieldProps = SelectHTMLAttributes<HTMLSelectElement> & {
  label: string;
  hint?: string;
  error?: string;
  children: ReactNode;
};

export const SelectField = forwardRef<HTMLSelectElement, SelectFieldProps>(
  ({ label, hint, error, children, ...props }, ref) => (
    <Field label={label} hint={hint} error={error}>
      {({ inputId, describedBy }) => (
        <select
          ref={ref}
          id={inputId}
          className="field__control"
          aria-invalid={Boolean(error)}
          aria-describedby={describedBy}
          {...props}
        >
          {children}
        </select>
      )}
    </Field>
  ),
);
SelectField.displayName = "SelectField";
