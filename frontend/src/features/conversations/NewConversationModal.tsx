import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { Button, Modal, SelectField, TextField } from "../../ui";
import { newConversationSchema } from "./newConversationSchema";
import type { NewConversationFormData } from "./newConversationSchema";
import { useOpenConversation } from "./hooks";
import { useAuthStore } from "../../stores/authStore";
import { toNullableText } from "../../lib/forms";

type NewConversationModalProps = { onClose: () => void; onCreated: (id: number) => void };

export function NewConversationModal({ onClose, onCreated }: NewConversationModalProps) {
  const userId = useAuthStore((s) => s.userId);
  const openConversation = useOpenConversation();
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<NewConversationFormData>({
    resolver: zodResolver(newConversationSchema),
    mode: "onBlur",
    defaultValues: { purpose: "Support", priority: "Normal", subject: "" },
  });

  async function onSubmit(data: NewConversationFormData) {
    if (!userId) return;
    const result = await openConversation.mutateAsync({
      customerId: userId,
      purpose: data.purpose,
      priority: data.priority,
      subject: toNullableText(data.subject ?? ""),
      sourcePageUrl: typeof window !== "undefined" ? window.location.href : null,
    });
    onCreated(result.id);
    onClose();
  }

  return (
    <Modal
      title="Nova conversa"
      onClose={onClose}
      actions={
        <>
          <Button variant="ghost" onClick={onClose} type="button">
            Cancelar
          </Button>
          <Button form="new-conversation-form" type="submit" disabled={isSubmitting} data-testid="new-conversation-submit">
            Iniciar
          </Button>
        </>
      }
    >
      <form id="new-conversation-form" className="stack" onSubmit={handleSubmit(onSubmit)}>
        <SelectField label="Assunto" data-testid="new-conversation-purpose" {...register("purpose")}>
          <option value="Support">Suporte</option>
          <option value="Sales">Vendas</option>
        </SelectField>
        <SelectField label="Prioridade" data-testid="new-conversation-priority" {...register("priority")}>
          <option value="Low">Baixa</option>
          <option value="Normal">Normal</option>
          <option value="High">Alta</option>
          <option value="Urgent">Urgente</option>
        </SelectField>
        <TextField
          label="Título (opcional)"
          placeholder="Ex.: Dúvida sobre pedido #123"
          error={errors.subject?.message}
          data-testid="new-conversation-subject"
          {...register("subject")}
        />
      </form>
    </Modal>
  );
}
