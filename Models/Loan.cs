using System;

namespace LibratyManagementSystem.Models
{
    public class Loan
    {
        public string LoanId { get; set; }
        public string CopyNumber { get; set; }
        public string UserNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } // "Active", "Returned"
    }
}
