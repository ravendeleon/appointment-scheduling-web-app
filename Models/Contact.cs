namespace SchedulingApp.Models
{
    public class Contact : Person
    {
        public string Phone { get; set; }
        public string Email { get; set; }

        public override string GetDisplayInfo()
        {
            return $"Contact: {Name} | Phone: {Phone} | Email: {Email}";
        }
    }
}