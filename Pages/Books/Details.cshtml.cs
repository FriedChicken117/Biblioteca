using System.ComponentModel.DataAnnotations;
using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Biblioteca.Pages.Books;

public class DetailsModel : PageModel
{
    private readonly XmlDataService _xmlDataService;

    public DetailsModel(XmlDataService xmlDataService)
    {
        _xmlDataService = xmlDataService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public Book? Book { get; set; }

    public IEnumerable<Review> Reviews { get; set; } = Enumerable.Empty<Review>();

    [BindProperty]
    public ReviewInputModel NewReview { get; set; } = new();

    public class ReviewInputModel
    {
        [Required]
        [Range(1,5)]
        public int Rating { get; set; }

        [Required]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        Book = _xmlDataService.GetBookById(Id);
        if (Book == null)
        {
            return NotFound();
        }

        Reviews = _xmlDataService.GetReviewsForBook(Id);
        return Page();
    }

    [Authorize]
    public IActionResult OnPost()
    {
        Book = _xmlDataService.GetBookById(Id);
        if (Book == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            Reviews = _xmlDataService.GetReviewsForBook(Id);
            return Page();
        }

        var userName = User.Identity?.Name ?? "Anónimo";

        var review = new Review
        {
            BookId = Id,
            UserName = userName,
            Rating = Math.Clamp(NewReview.Rating, 1, 5),
            Comment = NewReview.Comment
        };

        _xmlDataService.AddReview(review);

        return RedirectToPage(new { id = Id });
    }
}

