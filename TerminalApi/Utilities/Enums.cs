namespace TerminalApi.Utilities
{
    public enum EnumGender
    {
        Homme = 0,
        Femme = 1,
        NonBinaire = 2,
        Autre = 3,
    }

    public enum EnumSlotType
    {
        Presentiel = 0,
    }

    public enum EnumBookingStatus
    {
        Pending = 0,
        Paid = 1,
        Cancelled = 2,
    }
    public enum EnumTypeHelp
    {
        other = 0,
        Exams = 1,
        Homework = 2
    }
    public enum EnumNotificationType
    {
        // Account related
        AccountConfirmed = 0,
        PasswordChanged = 1,
        AccountUpdated = 2,
        PasswordResetDemandAccepted = 3,

        // Reservation related
        ReservationAccepted = 10,
        ReservationRejected = 11,
        ReservationCancelled = 12,
        ReservationCancelledTimeOut = 13,
        ReservationReminder = 14,
        NewReservation = 15,

        // Payment related
        PaymentAccepted = 20,
        PaymentFailed = 21,
        RefundProcessed = 22,

        // Teacher/Student interaction
        MessageReceived = 30,
        ReviewReceived = 31,
        NewAnnouncement = 32,

        // General notifications
        SystemUpdate = 40,
        PromotionOffer = 41,
        GeneralReminder = 42,
    }
}
