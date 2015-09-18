using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;

namespace HappyNotez.Models
{
    public class NotezContext : DbContext
    {
        public DbSet<Notez> Notez { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notez>().Property(b => b.Timestamp).Required();
            modelBuilder.Entity<Notez>().Property(b => b.Found).Required();
        }
    }

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
        // public DbGeography Location { get; set; }
        public string LocationRaw { get; set; }

        public double? Longitude { get; set; }
        public double? Latitude { get; set; }
        public float? Zoom { get; set; }
        public float? Elevation { get; set; }
    }
}
