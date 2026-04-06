using System;

namespace SchedulingApp.Models
{
    public class UserScheduleRow
    {
        public string UserName { get; set; }
        public int AppointmentId { get; set; }
        public string CustomerName { get; set; }
        public string Type { get; set; }
        public DateTime StartLocal { get; set; }
        public DateTime EndLocal { get; set; }
    }
}