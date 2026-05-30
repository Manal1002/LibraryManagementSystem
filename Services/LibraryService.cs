using System;
using System.Collections.Generic;
using System.Linq;
using LibratyManagementSystem.Models;

namespace LibratyManagementSystem.Services
{
    public class LibraryService
    {
        private readonly List<Book> _books = new List<Book>();
        private readonly List<User> _users = new List<User>();
        private readonly List<Loan> _loans = new List<Loan>();
        private readonly List<Reservation> _reservations = new List<Reservation>();

        public LibraryService()
        {
            SeedData();
        }

        // --- In-Memory Lists Accessors ---
        public List<Book> GetBooks() => _books;
        public List<User> GetUsers() => _users;
        public List<Loan> GetLoans() => _loans;
        public List<Reservation> GetReservations() => _reservations;

        // --- User Registration Process ---
        public User RegisterUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.UserNumber))
            {
                // Generate next User Number
                int nextId = _users.Count + 1;
                string prefix = user.UserType == "Member" ? "MEM" : "VIS";
                user.UserNumber = $"{prefix}{nextId:D4}";
            }
            _users.Add(user);
            return user;
        }

        // --- Book Registration Process ---
        public Book RegisterBook(string title, string author, string publisher, char classification, string genre, int totalCopies, int referenceCopiesCount)
        {
            classification = char.ToUpper(classification);
            
            // Generate book accession number: X 9999
            int countInClass = _books.Count(b => b.Classification == classification) + 1;
            string accessionNumber = $"{classification}{countInClass:D4}";

            var book = new Book
            {
                AccessionNumber = accessionNumber,
                Title = title,
                Author = author,
                Publisher = publisher,
                Classification = classification,
                Genre = genre
            };

            // Limit maximum of 10 copies
            int actualCopies = Math.Min(totalCopies, 10);
            for (int i = 1; i <= actualCopies; i++)
            {
                // Copy number format: X9999-N
                bool isRef = i <= referenceCopiesCount;
                book.Copies.Add(new BookCopy
                {
                    CopyNumber = $"{accessionNumber}-{i}",
                    BookAccessionNumber = accessionNumber,
                    IsReferenceOnly = isRef,
                    Status = "Available"
                });
            }

            _books.Add(book);
            return book;
        }

        // --- Loan Process ---
        public bool BorrowBook(string copyNumber, string userNumber, out string errorMessage)
        {
            errorMessage = "";

            // Find copy
            BookCopy copy = null;
            Book parentBook = null;
            foreach (var b in _books)
            {
                copy = b.Copies.FirstOrDefault(c => c.CopyNumber.Equals(copyNumber, StringComparison.OrdinalIgnoreCase));
                if (copy != null)
                {
                    parentBook = b;
                    break;
                }
            }

            if (copy == null)
            {
                errorMessage = "The requested book copy was not found in the catalog.";
                return false;
            }

            // Find user
            var user = _users.FirstOrDefault(u => u.UserNumber.Equals(userNumber, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                errorMessage = "The user registration details could not be found.";
                return false;
            }

            // Registered Visitors cannot borrow, only reference
            if (user.UserType.Equals("Visitor", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Registered Visitors are only allowed to refer books in the library. Borrowing is restricted to Registered Members.";
                return false;
            }

            // Check if copy is reference-only
            if (copy.IsReferenceOnly)
            {
                errorMessage = "This copy is marked as 'Reference Only' and cannot be loaned out of the library.";
                return false;
            }

            // Check if already loaned out
            if (copy.Status == "Loaned")
            {
                errorMessage = "This copy is currently loaned out to another member.";
                return false;
            }

            // Check if member already has 5 active loans
            int activeLoansCount = _loans.Count(l => l.UserNumber.Equals(userNumber, StringComparison.OrdinalIgnoreCase) && l.Status == "Active");
            if (activeLoansCount >= 5)
            {
                errorMessage = "Loan Limit Exceeded! Registered Members are only allowed a maximum of 5 active book loans simultaneously.";
                return false;
            }

            // Complete the loan
            copy.Status = "Loaned";
            
            var loan = new Loan
            {
                LoanId = $"L{(Guid.NewGuid().ToString().Substring(0, 6).ToUpper())}",
                CopyNumber = copy.CopyNumber,
                UserNumber = user.UserNumber,
                IssueDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14), // Borrow for 2 weeks
                Status = "Active"
            };

            _loans.Add(loan);
            return true;
        }

        // --- Return Process ---
        public bool ReturnBook(string copyNumber, out string alertMessage)
        {
            alertMessage = "";

            // Find active loan
            var loan = _loans.FirstOrDefault(l => l.CopyNumber.Equals(copyNumber, StringComparison.OrdinalIgnoreCase) && l.Status == "Active");
            if (loan == null)
            {
                alertMessage = "No active loan record found for this copy.";
                return false;
            }

            // Find copy
            BookCopy copy = null;
            foreach (var b in _books)
            {
                copy = b.Copies.FirstOrDefault(c => c.CopyNumber.Equals(copyNumber, StringComparison.OrdinalIgnoreCase));
                if (copy != null) break;
            }

            if (copy == null)
            {
                alertMessage = "Book copy database mismatch.";
                return false;
            }

            // Complete the return
            loan.ReturnDate = DateTime.Now;
            loan.Status = "Returned";

            // Check outstanding reservations for this book's title
            var pendingReservation = _reservations
                .Where(r => r.BookAccessionNumber.Equals(copy.BookAccessionNumber, StringComparison.OrdinalIgnoreCase) && r.Status == "Pending")
                .OrderBy(r => r.ReservedDate)
                .FirstOrDefault();

            if (pendingReservation != null)
            {
                // Reservation found! Put copy on one side (SetAside)
                copy.Status = "SetAside";
                pendingReservation.Status = "Notified";

                var reservingUser = _users.FirstOrDefault(u => u.UserNumber == pendingReservation.UserNumber);
                string reserverName = reservingUser != null ? reservingUser.Name : "Unknown Member";

                alertMessage = $"RESERVATION TRIGGERED! This copy has been reserved. Please set this copy aside. A notification has been sent to member {reserverName} ({pendingReservation.UserNumber}) who holds the oldest pending reservation.";
            }
            else
            {
                copy.Status = "Available";
                alertMessage = "Book returned successfully. No active reservations. The book has been added back to the available catalog.";
            }

            return true;
        }

        // --- Reservation Process ---
        public bool ReserveBook(string accessionNumber, string userNumber, out string errorMessage)
        {
            errorMessage = "";

            var book = _books.FirstOrDefault(b => b.AccessionNumber.Equals(accessionNumber, StringComparison.OrdinalIgnoreCase));
            if (book == null)
            {
                errorMessage = "The requested book title was not found in the catalog.";
                return false;
            }

            var user = _users.FirstOrDefault(u => u.UserNumber.Equals(userNumber, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                errorMessage = "The user registration details could not be found.";
                return false;
            }

            if (user.UserType.Equals("Visitor", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = "Registered Visitors are only allowed to refer books inside the library. Only Registered Members can make reservations.";
                return false;
            }

            // Check if member already has a pending reservation for this title
            var existingRes = _reservations.FirstOrDefault(r => 
                r.BookAccessionNumber.Equals(accessionNumber, StringComparison.OrdinalIgnoreCase) && 
                r.UserNumber.Equals(userNumber, StringComparison.OrdinalIgnoreCase) && 
                r.Status == "Pending"
            );

            if (existingRes != null)
            {
                errorMessage = "You already have an outstanding pending reservation for this book title.";
                return false;
            }

            // Log Reservation
            var reservation = new Reservation
            {
                ReservationId = $"R{(Guid.NewGuid().ToString().Substring(0, 6).ToUpper())}",
                BookAccessionNumber = book.AccessionNumber,
                UserNumber = user.UserNumber,
                ReservedDate = DateTime.Now,
                Status = "Pending"
            };

            _reservations.Add(reservation);
            return true;
        }

        // --- Inquiry & Search Engine ---
        public List<InquiryResultViewModel> SearchCatalog(string query)
        {
            var results = new List<InquiryResultViewModel>();
            var sourceList = _books;

            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                sourceList = _books.Where(b => 
                    b.AccessionNumber.ToLower().Contains(query) ||
                    b.Title.ToLower().Contains(query) ||
                    b.Author.ToLower().Contains(query) ||
                    b.Publisher.ToLower().Contains(query) ||
                    b.Genre.ToLower().Contains(query)
                ).ToList();
            }

            foreach (var book in sourceList)
            {
                int total = book.Copies.Count;
                int reference = book.Copies.Count(c => c.IsReferenceOnly);
                int borrowable = total - reference;
                int available = book.Copies.Count(c => c.Status == "Available" && !c.IsReferenceOnly);
                int loaned = book.Copies.Count(c => c.Status == "Loaned");
                int reserved = book.Copies.Count(c => c.Status == "SetAside");

                results.Add(new InquiryResultViewModel
                {
                    Book = book,
                    TotalCopies = total,
                    ReferenceCopies = reference,
                    BorrowableCopies = borrowable,
                    AvailableCopies = available,
                    LoanedCopies = loaned,
                    ReservedCopies = reserved
                });
            }

            return results;
        }

        // --- Private Pre-seed Data Method ---
        private void SeedData()
        {
            // Seed Users
            _users.Add(new User { UserNumber = "MEM0001", Name = "Amara Silva", Sex = "Male", NIC = "199412304910", Address = "Colombo 07, Sri Lanka", UserType = "Member" });
            _users.Add(new User { UserNumber = "MEM0002", Name = "Nethmi Perera", Sex = "Female", NIC = "199852208940", Address = "Kandy Road, Kadawatha", UserType = "Member" });
            _users.Add(new User { UserNumber = "MEM0003", Name = "Kasun Fernando", Sex = "Male", NIC = "199120409823", Address = "Galle Face, Colombo 03", UserType = "Member" });
            _users.Add(new User { UserNumber = "VIS0004", Name = "John Smith", Sex = "Male", NIC = "N89123098", Address = "Temporary Tourist Villa, Hikkaduwa", UserType = "Visitor" });
            _users.Add(new User { UserNumber = "VIS0005", Name = "Priyantha Kumara", Sex = "Male", NIC = "197523908492", Address = "Negombo Road, Kurunegala", UserType = "Visitor" });

            // Seed Books & Copies
            // Category: Science (S)
            RegisterBook("Access 2022 All-in-One Desk Reference", "Alan Simpson", "Wiley Publishing", 'S', "Technology", 4, 1); // 4 copies: 1 reference, 3 borrowable
            RegisterBook("Introduction to C# and .NET Core", "Mark J. Price", "Packt Publishing", 'S', "Computer Science", 3, 1);
            RegisterBook("Advanced Algorithms & Architectures", "Donald Knuth", "Addison-Wesley", 'S', "Mathematics", 2, 2); // 2 reference copies
            RegisterBook("A Brief History of Time", "Stephen Hawking", "Bantam Books", 'S', "Physics", 5, 1);
            RegisterBook("Design Patterns in C#", "Erich Gamma", "Pearson Education", 'S', "Software Engineering", 4, 1);

            // Category: Literature (L)
            RegisterBook("The Great Gatsby", "F. Scott Fitzgerald", "Charles Scribner's Sons", 'L', "Fiction", 3, 0);
            RegisterBook("To Kill a Mockingbird", "Harper Lee", "J.B. Lippincott & Co.", 'L', "Fiction", 2, 0);
            RegisterBook("Macbeth: Shakespearian Tragedy", "William Shakespeare", "Classic Books Inc.", 'L', "Drama", 4, 1);
            RegisterBook("Madol Doova (Sinhala Classic)", "Martin Wickramasinghe", "Sarasavi Publishers", 'L', "Adventure", 6, 1);
            RegisterBook("Gamperaliya", "Martin Wickramasinghe", "Sarasavi Publishers", 'L', "Drama", 3, 1);

            // Category: History (H)
            RegisterBook("Sapiens: A Brief History of Humankind", "Yuval Noah Harari", "Harper", 'H', "Anthropology", 4, 1);
            RegisterBook("Mahavamsa: The Great Chronicle of Sri Lanka", "Mahanama Thera", "State Printing Corp", 'H', "Ancient History", 2, 1);

            // Seed active loans
            // Amara borrows Access 2022 (S0001-2)
            BorrowBook("S0001-2", "MEM0001", out _);
            // Nethmi borrows Great Gatsby (L0001-1)
            BorrowBook("L0001-1", "MEM0002", out _);
            // Kasun borrows Madol Doova (L0004-2)
            BorrowBook("L0004-2", "MEM0003", out _);

            // Seed reservations
            // Amara reserves Great Gatsby because she wants to read it next
            _reservations.Add(new Reservation
            {
                ReservationId = "R00001",
                BookAccessionNumber = "L0001",
                UserNumber = "MEM0001",
                ReservedDate = DateTime.Now.AddDays(-1),
                Status = "Pending"
            });
        }
    }

    public class InquiryResultViewModel
    {
        public Book Book { get; set; }
        public int TotalCopies { get; set; }
        public int ReferenceCopies { get; set; }
        public int BorrowableCopies { get; set; }
        public int AvailableCopies { get; set; }
        public int LoanedCopies { get; set; }
        public int ReservedCopies { get; set; }
    }
}
