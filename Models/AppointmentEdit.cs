using System;

namespace SchedulingApp.Models
{
    public class AppointmentEdit
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Contact { get; set; }
        public string Location { get; set; }
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public int UserId { get; set; }
    }
}