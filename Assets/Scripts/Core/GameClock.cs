using System;
using UnityEngine;

namespace SundayLastBreath.Core
{
    /// <summary>
    /// Tracks in-game world time, clock rollover, and formatted time stamps.
    /// Starts at Saturday, Oct 25, 2025 at 23:45.
    /// Actions advance time toward Sunday midnight (the collapse).
    /// </summary>
    [Serializable]
    public class GameClock
    {
        [SerializeField] private string dayName = "SATURDAY";
        [SerializeField] private string month = "OCT";
        [SerializeField] private int date = 25;
        [SerializeField] private int year = 2025;
        [SerializeField] private int hour = 23;
        [SerializeField] private int minute = 45;
        [SerializeField] private int dayCount = 0;

        public string DayName => dayName;
        public string Month => month;
        public int Date => date;
        public int Year => year;
        public int Hour => hour;
        public int Minute => minute;
        public int DayCount => dayCount;

        private static readonly string[] DaysOfWeek = { "SATURDAY", "SUNDAY", "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY" };

        public event Action<GameClock> OnTimeAdvanced;

        public GameClock(int hour = 23, int minute = 45, int date = 25, string month = "OCT", int year = 2025, string dayName = "SATURDAY")
        {
            this.hour = hour;
            this.minute = minute;
            this.date = date;
            this.month = month;
            this.year = year;
            this.dayName = dayName;
            this.dayCount = 0;
        }

        /// <summary>
        /// Advances in-game time by the specified number of minutes.
        /// Handles rollover from 59 mins -> hours and 23 hours -> next day.
        /// </summary>
        public void AddTime(int minutesToAdd)
        {
            if (minutesToAdd <= 0) return;

            minute += minutesToAdd;

            while (minute >= 60)
            {
                minute -= 60;
                hour += 1;
            }

            while (hour >= 24)
            {
                hour -= 24;
                dayCount += 1;
                date += 1;
                dayName = DaysOfWeek[dayCount % 7];
            }

            OnTimeAdvanced?.Invoke(this);
        }

        public string GetTimeString()
        {
            return $"{hour:D2}:{minute:D2}";
        }

        public string GetFullDateString()
        {
            return $"{dayName}, {month} {date:D2}, {year} // {hour:D2}:{minute:D2}";
        }

        public string GetDayTimeString()
        {
            string timePeriod = (hour >= 20 || hour < 5) ? "NIGHT" : (hour < 12 ? "MORNING" : "AFTERNOON");
            return $"{dayName} {timePeriod}  |  {hour:D2}:{minute:D2}";
        }
    }
}

