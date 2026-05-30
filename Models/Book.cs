using System.Collections.Generic;

namespace LibratyManagementSystem.Models
{
    public class Book
    {
        public string AccessionNumber { get; set; } // e.g., "S0001"
        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public char Classification { get; set; } // e.g., 'S', 'L'
        public string Genre { get; set; }
        public List<BookCopy> Copies { get; set; } = new List<BookCopy>();
    }

    public class BookCopy
    {
        public string CopyNumber { get; set; } // e.g., "S0001-1", "S0001-2"
        public string BookAccessionNumber { get; set; }
        public bool IsReferenceOnly { get; set; } // True = Reference (only refer), False = Borrowable
        public string Status { get; set; } // "Available", "Loaned", "Reserved", "SetAside" (held for reservation fulfillment)
    }
}
