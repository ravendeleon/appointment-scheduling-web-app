using System;

namespace SchedulingApp.Models
{
    public class Appointment
    {
        public int AppointmentId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Contact { get; set; }
        public string Location { get; set; }
        public DateTime StartLocal { get; set; }
        public DateTime EndLocal { get; set; }
        public int UserId { get; set; }
    }
}