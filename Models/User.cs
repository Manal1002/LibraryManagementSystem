namespace LibratyManagementSystem.Models
{
    public class User
    {
        public string UserNumber { get; set; } // Unique User ID
        public string Name { get; set; }
        public string Sex { get; set; } // "Male", "Female", "Other"
        public string NIC { get; set; } // National Identity Card number
        public string Address { get; set; }
        public string UserType { get; set; } // "Member" (can borrow), "Visitor" (reference only)
    }
}
