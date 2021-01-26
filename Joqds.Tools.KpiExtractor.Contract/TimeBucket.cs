using System;
using System.Collections.Generic;
using System.Globalization;

namespace Joqds.Tools.KpiExtractor.Contract
{
    public class TimeBucket<TCalendar>:IComparable<TimeBucket<TCalendar>> where TCalendar:Calendar, new()
    {
        public static IEnumerable<TimeBucket<TCalendar>> GetBuckets(DateTime start, DateTime end, DatePart datePart,
            bool startingPartial = false, bool endingPartial = false)
        {
            var timeBuckets = new List<TimeBucket<TCalendar>>();
            var firstBucket = new TimeBucket<TCalendar>(start,datePart,partialStart: startingPartial);
            var lastBucket = new TimeBucket<TCalendar>(end,datePart,partialEnd: endingPartial);
            TimeBucket<TCalendar> current;
            if(firstBucket.Given==firstBucket.StartPoint || startingPartial)
            {
                timeBuckets.Add(firstBucket);
                current = firstBucket.Clone();
            }
            else
            {
                current = firstBucket.AddDatePart(1).Clone();
            }

            if (lastBucket.Given != lastBucket.EndPoint && !endingPartial)
            {
                lastBucket.AddDatePart(-1);
            }

            while (current.EndPoint<lastBucket.StartPoint)
            {
                timeBuckets.Add(current.AddDatePart(1).Clone());
            }

            timeBuckets.Add(lastBucket);

            return timeBuckets;
        }

        static TimeBucket()
        {
            _calendar = new TCalendar();
        }

        private static readonly Calendar _calendar;
        public TimeBucket(DateTime given,DatePart datePart, bool partialStart=false, bool partialEnd=false)
        {
            //todo: exact 0 must not increment or decrement start, end
            Given = given;
            DatePart = datePart;
            PartialStart = partialStart;
            PartialEnd = partialEnd;
            EndPoint = datePart switch
            {
                DatePart.Minute => new DateTime(Given.Year, Given.Month, Given.Day, Given.Hour,
                    Given.Minute, 0).AddMinutes(1),
                DatePart.Hour => new DateTime(Given.Year, Given.Month, Given.Day, Given.Hour, 0, 0)
                    .AddHours(1),
                DatePart.Day => new DateTime(Given.Year, Given.Month, Given.Day).AddDays(1),
                DatePart.Week => new DateTime(Given.Year, Given.Month, Given.Day)
                    .AddDays((int)DayOfWeek.Saturday - (int)Given.DayOfWeek)
                    .AddDays(7),
                DatePart.Month => _calendar.AddMonths(
                    new DateTime(Given.Year, Given.Month, Given.Day).AddDays(
                        (_calendar.GetDayOfMonth(Given) - 1) * -1), 1),
                DatePart.Year => _calendar.AddYears(
                    new DateTime(Given.Year, Given.Month, Given.Day).AddDays(
                        (_calendar.GetDayOfYear(Given) - 1) * -1), 1),
                _ => throw new ArgumentOutOfRangeException(nameof(datePart), datePart, null)
            };

            StartPoint = datePart switch
            {
                DatePart.Minute => new DateTime(Given.Year, Given.Month, Given.Day, Given.Hour,
                    Given.Minute, 0),
                DatePart.Hour => new DateTime(Given.Year, Given.Month, Given.Day, Given.Hour, 0, 0),
                DatePart.Day => new DateTime(Given.Year, Given.Month, Given.Day),
                DatePart.Week => new DateTime(Given.Year, Given.Month, Given.Day).AddDays(
                    (int)DayOfWeek.Saturday - (int)Given.DayOfWeek),
                DatePart.Month => new DateTime(Given.Year, Given.Month, Given.Day).AddDays(
                    (_calendar.GetDayOfMonth(Given) - 1) * -1),
                DatePart.Year => new DateTime(Given.Year, Given.Month, Given.Day).AddDays(
                    (_calendar.GetDayOfYear(Given) - 1) * -1),
                _ => throw new ArgumentOutOfRangeException(nameof(datePart), datePart, null)
            };
        }
        public DateTime StartPoint { get; set; }
        public DateTime EndPoint { get; set; }
        private bool PartialStart { get; set; }
        private bool PartialEnd { get; set; }
        public DateTime Start => PartialStart ? Given : StartPoint;
        public DateTime End => PartialEnd ? Given : EndPoint;
        public DateTime Given { get; set; }
        public DatePart DatePart { get; set; }

        public bool IsPartial => PartialStart || PartialEnd;

        public TimeBucket<TCalendar> AddDatePart(int value)
        {
            Given = StartPoint = DatePart switch
            {
                DatePart.Minute => StartPoint.AddMinutes(value),
                DatePart.Hour => StartPoint.AddHours(value),
                DatePart.Day => StartPoint.AddDays(value),
                DatePart.Week => StartPoint.AddDays(7 * value),
                DatePart.Month => _calendar.AddMonths(StartPoint, value),
                DatePart.Year => _calendar.AddYears(StartPoint, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(Contract.DatePart), DatePart, null)
            };
            EndPoint = DatePart switch
            {
                DatePart.Minute => EndPoint.AddMinutes(value),
                DatePart.Hour => EndPoint.AddHours(value),
                DatePart.Day => EndPoint.AddDays(value),
                DatePart.Week => EndPoint.AddDays(7 * value),
                DatePart.Month => _calendar.AddMonths(EndPoint, value),
                DatePart.Year => _calendar.AddYears(EndPoint, 1),
                _ => throw new ArgumentOutOfRangeException(nameof(Contract.DatePart), DatePart, null)
            };
            PartialStart = PartialEnd = false;
            return this;
        }

        public TimeBucket<TCalendar> Clone()
        {
            return new TimeBucket<TCalendar>(Given,DatePart,PartialStart,PartialEnd);
        }

        public int CompareTo(TimeBucket<TCalendar> other)
        {
            return StartPoint.CompareTo(other.StartPoint);
        }
    }
}