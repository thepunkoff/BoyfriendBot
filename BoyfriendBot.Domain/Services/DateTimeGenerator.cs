using BoyfriendBot.Domain.AppSettings;
using BoyfriendBot.Domain.Core;
using BoyfriendBot.Domain.Core.Extensions;
using BoyfriendBot.Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace BoyfriendBot.Domain.Services
{
    public class DateTimeGenerator : IDateTimeGenerator
    {
        private readonly DateTimeGeneratorAppSettings _appSettings;
        private readonly ILogger<DateTimeGenerator> _logger;

        public DateTimeGenerator(
              IOptions<DateTimeGeneratorAppSettings> appSettings
            , ILogger<DateTimeGenerator> logger
            )
        {
            _appSettings = appSettings.Value;
            _logger = logger;
        }

        public List<DateTime> GenerateDateTimesWithinRange(TimeSpanRange range, int messageCount)
        {
            _logger.LogInformation($"Generating schedule date times with the method: \"{_appSettings.DateTimeGenerationMethod}\"");

            List<DateTime> dateTimes = null;
            switch (_appSettings.DateTimeGenerationMethod)
            {
                case "Random":
                    dateTimes = GenerateRandomDateTimesWithinRange(range, messageCount);
                    break;

                case "Smart":
                    dateTimes = GenerateSmartDateTimesWithinRange(range, messageCount);
                    break;

                default:
                    dateTimes = GenerateRandomDateTimesWithinRange(range, messageCount);
                    break;
            }

            _logger.LogInformation("Messages generated:");
            foreach (var dt in dateTimes)
            {
                _logger.LogInformation($"{dt.PartOfDay().Name} - {dt}");
            }

            return dateTimes;
        }
        private List<DateTime> GenerateRandomDateTimesWithinRange(TimeSpanRange schedulingRange, int messageCount)
        {
            var dateTimes = new List<DateTime>();

            var rng = new Random();

            var schedulingDay = GetSchedulingDay(schedulingRange);

            for (int i = 0; i < messageCount; i++)
            {
                var maxSeconds = (int)schedulingRange.Difference.TotalSeconds;

                var randomDateTime = schedulingDay.Add(schedulingRange.Start).AddSeconds(rng.Next(maxSeconds));
                dateTimes.Add(randomDateTime);
            }

            dateTimes.Sort();

            return dateTimes;
        }

        private List<DateTime> GenerateSmartDateTimesWithinRange(TimeSpanRange schedulingRange, int messageCount)
        {
            var schedulingDay = GetSchedulingDay(schedulingRange);

            var dateTimes = new List<DateTime>();

            var biasRng = new Random();

            var step = schedulingRange.Difference.TotalSeconds / messageCount;
            var randomSecondsBias = biasRng.NextDouble() * step;

            for (double seconds = schedulingRange.Start.TotalSeconds; seconds < schedulingRange.End.TotalSeconds; seconds += step)
            {
                var noiseRng = new Random();
                var noiseValue = step / _appSettings.StepToNoiseRatio;
                var noiseRangeStart = -noiseValue;
                var noiseRangeEnd = noiseValue;
                var randomSecondsNoise = noiseRng.NextDouble() * (noiseRangeEnd - noiseRangeStart) + noiseRangeStart;

                // actually modulo needed just in case noise overflows maxvalue
                var dateTime = schedulingDay.AddSeconds(seconds + ((randomSecondsBias + randomSecondsNoise) % schedulingRange.Difference.TotalSeconds));

                dateTimes.Add(dateTime);
            }

            dateTimes.Sort();

            return dateTimes;
        }

        private DateTime GetSchedulingDay(TimeSpanRange schedulingRange)
        {
            var schedulingPartOfDay = DateTime.MinValue.Add(schedulingRange.Start).PartOfDay(); // choose any date with any time within the range to determine ToD

            if (schedulingPartOfDay.Name == Const.PartOfDay.Night)
            {
                return DateTime.Now.Date.AddDays(1);
            }
            else
            {
                return DateTime.Now.Date;
            }
        }
    }
}
