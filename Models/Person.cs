namespace SchedulingApp.Models
{

    public abstract class Person
    {

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value?.Trim() ?? string.Empty; }
        }

        public virtual string GetDisplayInfo()
        {
            return $"Name: {Name}";
        }
    }
}