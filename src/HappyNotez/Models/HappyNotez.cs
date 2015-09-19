using System;

namespace HappyNotez.Models
{
    public enum FlagStatus : byte
    {
        NotFlaggged = 0,
        Flagged = 1,
        Approved = 2,
        Removed = 3,
        Invalid = 5
    }

    public class Notez
    {
        public long ID { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public bool Found { get; set; }
        public FlagStatus FlagStatus { get; set; }
        public int ReportCount { get; set; }
        public string HostAddress { get; set; }
        public string UserAgent { get; set; }
        public string LocationRaw { get; set; }

        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public float? Zoom { get; set; }
        public float? Elevation { get; set; }
    }
}
