namespace SchedulingApp.Models
{
    // Customer inherits from Person
    public class Customer : Person
    {
        public int CustomerId { get; set; }

        // CustomerName maps to the inherited Name property from Person
        public string CustomerName
        {
            get { return Name; }
            set { Name = value; }
        }

        public string Address { get; set; }
        public string Phone { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public int CityId { get; set; }
        public string PostalCode { get; set; }

        // overrides the base class method - this is polymorphism
        public override string GetDisplayInfo()
        {
            return $"Customer: {Name} | City: {City} | Phone: {Phone}";
        }
    }
}