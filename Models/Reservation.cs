using System;

namespace LibratyManagementSystem.Models
{
    public class Reservation
    {
        public string ReservationId { get; set; }
        public string BookAccessionNumber { get; set; }
        public string UserNumber { get; set; }
        public DateTime ReservedDate { get; set; }
        public string Status { get; set; } // "Pending", "Notified", "Fulfilled", "Cancelled"
    }
}
