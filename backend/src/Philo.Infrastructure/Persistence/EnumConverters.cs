using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Philo.Domain.Enums;

namespace Philo.Infrastructure.Persistence
{
    /// <summary>
    /// Conversores explícitos enum <-> string, pois os enums MySQL usam valores em snake_case/minúsculo
    /// (ex.: 'waiting_customer') enquanto os enums .NET usam PascalCase (WaitingCustomer). Usa apenas
    /// operadores condicionais (sem switch expression) porque o EF precisa converter isto em Expression Tree.
    /// </summary>
    internal static class EnumConverters
    {
        public static readonly ValueConverter<OrganizationStatus, string> OrganizationStatusConverter = new(
            v => v == OrganizationStatus.Active ? "active" : "suspended",
            v => v == "active" ? OrganizationStatus.Active : OrganizationStatus.Suspended);

        public static readonly ValueConverter<UserStatus, string> UserStatusConverter = new(
            v => v == UserStatus.Active ? "active" : v == UserStatus.Suspended ? "suspended" : "anonymized",
            v => v == "active" ? UserStatus.Active : v == "suspended" ? UserStatus.Suspended : UserStatus.Anonymized);

        public static readonly ValueConverter<MembershipStatus, string> MembershipStatusConverter = new(
            v => v == MembershipStatus.Active ? "active" : "suspended",
            v => v == "active" ? MembershipStatus.Active : MembershipStatus.Suspended);

        public static readonly ValueConverter<Role, string> RoleConverter = new(
            v => v == Role.Customer ? "customer" : v == Role.Agent ? "agent" : "admin",
            v => v == "customer" ? Role.Customer : v == "agent" ? Role.Agent : Role.Admin);

        public static readonly ValueConverter<ProductStatus, string> ProductStatusConverter = new(
            v => v == ProductStatus.Active ? "active" : "inactive",
            v => v == "active" ? ProductStatus.Active : ProductStatus.Inactive);

        public static readonly ValueConverter<ConversationPurpose, string> ConversationPurposeConverter = new(
            v => v == ConversationPurpose.Sales ? "sales" : "support",
            v => v == "sales" ? ConversationPurpose.Sales : ConversationPurpose.Support);

        public static readonly ValueConverter<ConversationStatus, string> ConversationStatusConverter = new(
            v => v == ConversationStatus.Open ? "open"
                : v == ConversationStatus.WaitingCustomer ? "waiting_customer"
                : v == ConversationStatus.WaitingTeam ? "waiting_team"
                : "closed",
            v => v == "open" ? ConversationStatus.Open
                : v == "waiting_customer" ? ConversationStatus.WaitingCustomer
                : v == "waiting_team" ? ConversationStatus.WaitingTeam
                : ConversationStatus.Closed);

        public static readonly ValueConverter<Priority, string> PriorityConverter = new(
            v => v == Priority.Low ? "low" : v == Priority.Normal ? "normal" : v == Priority.High ? "high" : "urgent",
            v => v == "low" ? Priority.Low : v == "normal" ? Priority.Normal : v == "high" ? Priority.High : Priority.Urgent);

        public static readonly ValueConverter<MessageType, string> MessageTypeConverter = new(
            v => v == MessageType.Text ? "text" : "attachment",
            v => v == "text" ? MessageType.Text : MessageType.Attachment);

        public static readonly ValueConverter<ScanStatus, string> ScanStatusConverter = new(
            v => v == ScanStatus.Pending ? "pending" : v == ScanStatus.Clean ? "clean" : "rejected",
            v => v == "pending" ? ScanStatus.Pending : v == "clean" ? ScanStatus.Clean : ScanStatus.Rejected);

        public static readonly ValueConverter<ConversationEventType, string> ConversationEventTypeConverter = new(
            v => v == ConversationEventType.Created ? "created"
                : v == ConversationEventType.Assigned ? "assigned"
                : v == ConversationEventType.StatusChanged ? "status_changed"
                : v == ConversationEventType.ParticipantJoined ? "participant_joined"
                : "participant_left",
            v => v == "created" ? ConversationEventType.Created
                : v == "assigned" ? ConversationEventType.Assigned
                : v == "status_changed" ? ConversationEventType.StatusChanged
                : v == "participant_joined" ? ConversationEventType.ParticipantJoined
                : ConversationEventType.ParticipantLeft);
    }
}
