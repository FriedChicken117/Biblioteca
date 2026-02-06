using System.Xml.Linq;
using Biblioteca.Models;

namespace Biblioteca.Services;

public class XmlDataService
{
    private readonly string _dataDirectory;
    private readonly string _booksPath;
    private readonly string _reviewsPath;
    private readonly string _usersPath;
    private readonly object _lock = new();

    public XmlDataService(IWebHostEnvironment env)
    {
        _dataDirectory = Path.Combine(env.ContentRootPath, "Data");
        Directory.CreateDirectory(_dataDirectory);

        _booksPath = Path.Combine(_dataDirectory, "books.xml");
        _reviewsPath = Path.Combine(_dataDirectory, "reviews.xml");
        _usersPath = Path.Combine(_dataDirectory, "users.xml");

        EnsureFilesExist();
        EnsureAdminUserIsMarked();
    }

    private void EnsureFilesExist()
    {
        if (!File.Exists(_booksPath))
        {
            var doc = new XDocument(new XElement("Books",
                new XElement("Book",
                    new XElement("Id", 1),
                    new XElement("Title", "El Quijote"),
                    new XElement("Author", "Miguel de Cervantes"),
                    new XElement("Category", "Clásico"),
                    new XElement("Summary", "Las aventuras de Don Quijote y Sancho Panza.")
                ),
                new XElement("Book",
                    new XElement("Id", 2),
                    new XElement("Title", "Cien años de soledad"),
                    new XElement("Author", "Gabriel García Márquez"),
                    new XElement("Category", "Realismo mágico"),
                    new XElement("Summary", "La historia de la familia Buendía en Macondo.")
                )
            ));
            doc.Save(_booksPath);
        }

        if (!File.Exists(_reviewsPath))
        {
            new XDocument(new XElement("Reviews")).Save(_reviewsPath);
        }

        if (!File.Exists(_usersPath))
        {
            var admin = new User
            {
                Id = 1,
                UserName = "admin",
                PasswordHash = AuthService.HashPassword("admin"),
                IsAdmin = true
            };

            var doc = new XDocument(new XElement("Users",
                new XElement("User",
                    new XElement("Id", admin.Id),
                    new XElement("UserName", admin.UserName),
                    new XElement("PasswordHash", admin.PasswordHash),
                    new XElement("IsAdmin", admin.IsAdmin)
                )
            ));
            doc.Save(_usersPath);
        }
    }

    private void EnsureAdminUserIsMarked()
    {
        if (!File.Exists(_usersPath))
        {
            return;
        }

        var doc = XDocument.Load(_usersPath);
        var users = doc.Root!.Elements("User").ToList();
        if (!users.Any())
        {
            return;
        }

        foreach (var user in users)
        {
            var userName = (string?)user.Element("UserName") ?? string.Empty;
            var isAdminElement = user.Element("IsAdmin");
            var shouldBeAdmin = string.Equals(userName, "admin", StringComparison.OrdinalIgnoreCase);

            if (isAdminElement == null)
            {
                user.Add(new XElement("IsAdmin", shouldBeAdmin));
            }
            else
            {
                isAdminElement.Value = shouldBeAdmin.ToString();
            }
        }

        doc.Save(_usersPath);
    }

    public IEnumerable<Book> GetBooks(string? search, string? author, string? category)
    {
        var doc = XDocument.Load(_booksPath);
        var query = doc.Root!.Elements("Book").Select(x => new Book
        {
            Id = (int)x.Element("Id")!,
            Title = (string?)x.Element("Title") ?? string.Empty,
            Author = (string?)x.Element("Author") ?? string.Empty,
            Category = (string?)x.Element("Category") ?? string.Empty,
            Summary = (string?)x.Element("Summary") ?? string.Empty
        }).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(b =>
                b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category.Contains(category, StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderBy(b => b.Title).ToList();
    }

    public Book? GetBookById(int id)
    {
        var doc = XDocument.Load(_booksPath);
        var element = doc.Root!.Elements("Book").FirstOrDefault(x => (int)x.Element("Id")! == id);
        if (element == null) return null;
        return new Book
        {
            Id = (int)element.Element("Id")!,
            Title = (string?)element.Element("Title") ?? string.Empty,
            Author = (string?)element.Element("Author") ?? string.Empty,
            Category = (string?)element.Element("Category") ?? string.Empty,
            Summary = (string?)element.Element("Summary") ?? string.Empty
        };
    }

    public IEnumerable<Review> GetReviewsForBook(int bookId)
    {
        var doc = XDocument.Load(_reviewsPath);
        return doc.Root!.Elements("Review")
            .Where(x => (int)x.Element("BookId")! == bookId)
            .Select(x => new Review
            {
                Id = (int)x.Element("Id")!,
                BookId = (int)x.Element("BookId")!,
                UserName = (string?)x.Element("UserName") ?? string.Empty,
                Rating = (int)x.Element("Rating")!,
                Comment = (string?)x.Element("Comment") ?? string.Empty,
                CreatedAt = DateTime.Parse((string?)x.Element("CreatedAt") ?? DateTime.UtcNow.ToString("o"))
            })
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public void AddReview(Review review)
    {
        lock (_lock)
        {
            var doc = XDocument.Load(_reviewsPath);
            var nextId = doc.Root!.Elements("Review").Select(x => (int)x.Element("Id")!).DefaultIfEmpty(0).Max() + 1;
            review.Id = nextId;
            review.CreatedAt = DateTime.UtcNow;

            doc.Root.Add(new XElement("Review",
                new XElement("Id", review.Id),
                new XElement("BookId", review.BookId),
                new XElement("UserName", review.UserName),
                new XElement("Rating", review.Rating),
                new XElement("Comment", review.Comment),
                new XElement("CreatedAt", review.CreatedAt.ToString("o"))
            ));
            doc.Save(_reviewsPath);
        }
    }

    public User? GetUserByUserName(string userName)
    {
        var doc = XDocument.Load(_usersPath);
        var element = doc.Root!.Elements("User")
            .FirstOrDefault(x => string.Equals((string?)x.Element("UserName"), userName, StringComparison.OrdinalIgnoreCase));
        if (element == null) return null;
        return new User
        {
            Id = (int)element.Element("Id")!,
            UserName = (string?)element.Element("UserName") ?? string.Empty,
            PasswordHash = (string?)element.Element("PasswordHash") ?? string.Empty,
            IsAdmin = bool.TryParse((string?)element.Element("IsAdmin"), out var isAdmin) && isAdmin
        };
    }

    public void AddUser(User user)
    {
        lock (_lock)
        {
            var doc = XDocument.Load(_usersPath);
            var existing = doc.Root!.Elements("User")
                .FirstOrDefault(x => string.Equals((string?)x.Element("UserName"), user.UserName, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                throw new InvalidOperationException("El nombre de usuario ya existe.");
            }

            var nextId = doc.Root.Elements("User").Select(x => (int)x.Element("Id")!).DefaultIfEmpty(0).Max() + 1;
            user.Id = nextId;

            doc.Root.Add(new XElement("User",
                new XElement("Id", user.Id),
                new XElement("UserName", user.UserName),
                new XElement("PasswordHash", user.PasswordHash),
                new XElement("IsAdmin", user.IsAdmin)
            ));
            doc.Save(_usersPath);
        }
    }

    public void AddBook(Book book)
    {
        lock (_lock)
        {
            var doc = XDocument.Load(_booksPath);
            var nextId = doc.Root!.Elements("Book").Select(x => (int)x.Element("Id")!).DefaultIfEmpty(0).Max() + 1;
            book.Id = nextId;

            doc.Root.Add(new XElement("Book",
                new XElement("Id", book.Id),
                new XElement("Title", book.Title),
                new XElement("Author", book.Author),
                new XElement("Category", book.Category),
                new XElement("Summary", book.Summary)
            ));
            doc.Save(_booksPath);
        }
    }

    public IEnumerable<string> GetCategories()
    {
        var doc = XDocument.Load(_booksPath);
        return doc.Root!.Elements("Book")
            .Select(x => (string?)x.Element("Category") ?? string.Empty)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    public IEnumerable<Review> GetReviewsByUser(string userName)
    {
        var doc = XDocument.Load(_reviewsPath);
        return doc.Root!.Elements("Review")
            .Where(x => string.Equals((string?)x.Element("UserName"), userName, StringComparison.OrdinalIgnoreCase))
            .Select(x => new Review
            {
                Id = (int)x.Element("Id")!,
                BookId = (int)x.Element("BookId")!,
                UserName = (string?)x.Element("UserName") ?? string.Empty,
                Rating = (int)x.Element("Rating")!,
                Comment = (string?)x.Element("Comment") ?? string.Empty,
                CreatedAt = DateTime.Parse((string?)x.Element("CreatedAt") ?? DateTime.UtcNow.ToString("o"))
            })
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
    }

    public Review? GetReviewById(int reviewId)
    {
        var doc = XDocument.Load(_reviewsPath);
        var element = doc.Root!.Elements("Review").FirstOrDefault(x => (int)x.Element("Id")! == reviewId);
        if (element == null) return null;
        return new Review
        {
            Id = (int)element.Element("Id")!,
            BookId = (int)element.Element("BookId")!,
            UserName = (string?)element.Element("UserName") ?? string.Empty,
            Rating = (int)element.Element("Rating")!,
            Comment = (string?)element.Element("Comment") ?? string.Empty,
            CreatedAt = DateTime.Parse((string?)element.Element("CreatedAt") ?? DateTime.UtcNow.ToString("o"))
        };
    }

    public void UpdateReview(Review review)
    {
        lock (_lock)
        {
            var doc = XDocument.Load(_reviewsPath);
            var element = doc.Root!.Elements("Review").FirstOrDefault(x => (int)x.Element("Id")! == review.Id);
            if (element == null)
            {
                throw new InvalidOperationException("Reseña no encontrada.");
            }

            element.Element("Rating")!.Value = review.Rating.ToString();
            element.Element("Comment")!.Value = review.Comment;
            doc.Save(_reviewsPath);
        }
    }

    public void DeleteReview(int reviewId)
    {
        lock (_lock)
        {
            var doc = XDocument.Load(_reviewsPath);
            var element = doc.Root!.Elements("Review").FirstOrDefault(x => (int)x.Element("Id")! == reviewId);
            if (element == null)
            {
                throw new InvalidOperationException("Reseña no encontrada.");
            }

            element.Remove();
            doc.Save(_reviewsPath);
        }
    }
}

