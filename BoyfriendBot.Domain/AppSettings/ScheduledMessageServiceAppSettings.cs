namespace BoyfriendBot.Domain.AppSettings
{
    public class ScheduledMessageServiceAppSettings
    {
        public bool ScheduledMessageServiceOn { get; set; }
        public bool WakeUpMessageOn { get; set; }
        public bool ScheduleInMorning { get; set; }
        public int MorningMessagesCount { get; set; }
        public bool ScheduleInAfternoon { get; set; }
        public int AfternoonMessagesCount { get; set; }
        public bool ScheduleInEvening { get; set; }
        public int EveningMessagesCount { get; set; }
        public bool ScheduleInNight { get; set; }
        public int NightMessagesCount { get; set; }
        public double ThresholdInHours { get; set; }
        public double OverrideCategoryChanceNormalized { get; set; } // the chance of morning/afternoon/evening/night become the "ANY" category
        public int DefaultWhiteWeight { get; set; }
        public int DefaultGreenWeight { get; set; }
        public int DefaultBlueWeight { get; set; }
        public int DefaultPurpleWeight { get; set; }
        public int DefaultOrangeWeight { get; set; }
    }
}
