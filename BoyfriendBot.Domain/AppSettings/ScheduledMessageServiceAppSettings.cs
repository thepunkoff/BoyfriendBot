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

        public int ThresholdInHours { get; set; }
    }
}
