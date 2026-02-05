using System.ComponentModel.DataAnnotations;
using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Biblioteca.Pages.Account;

[Authorize]
public class MyReviewsModel : PageModel
{
    private readonly XmlDataService _xmlDataService;

    public MyReviewsModel(XmlDataService xmlDataService)
    {
        _xmlDataService = xmlDataService;
    }

    public IEnumerable<ReviewWithBook> Reviews { get; set; } = Enumerable.Empty<ReviewWithBook>();
    public Review? ReviewToEdit { get; set; }

    public class ReviewWithBook
    {
        public Review Review { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }

    public class EditInputModel
    {
        [Required]
        [Range(1, 5, ErrorMessage = "La calificación debe estar entre 1 y 5.")]
        public int Rating { get; set; }

        [Required]
        [Display(Name = "Comentario")]
        public string Comment { get; set; } = string.Empty;
    }

    [BindProperty]
    public EditInputModel EditInput { get; set; } = new();

    public int? EditReviewId { get; set; }

    public void OnGet(int? editId = null)
    {
        var userName = User.Identity?.Name ?? string.Empty;
        var reviews = _xmlDataService.GetReviewsByUser(userName);
        
        Reviews = reviews.Select(r =>
        {
            var book = _xmlDataService.GetBookById(r.BookId);
            return new ReviewWithBook
            {
                Review = r,
                Book = book ?? new Book { Id = r.BookId, Title = "Libro no encontrado" }
            };
        }).ToList();

        if (editId.HasValue)
        {
            var review = reviews.FirstOrDefault(r => r.Id == editId.Value);
            if (review != null && review.UserName == userName)
            {
                ReviewToEdit = review;
                EditReviewId = editId.Value;
                EditInput.Rating = review.Rating;
                EditInput.Comment = review.Comment;
            }
        }
    }

    public IActionResult OnPostEdit(int id)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToPage(new { editId = id });
        }

        var userName = User.Identity?.Name ?? string.Empty;
        var review = _xmlDataService.GetReviewById(id);
        
        if (review == null || review.UserName != userName)
        {
            return Forbid();
        }

        review.Rating = EditInput.Rating;
        review.Comment = EditInput.Comment;
        _xmlDataService.UpdateReview(review);

        return RedirectToPage();
    }

    public IActionResult OnPostDelete(int id)
    {
        var userName = User.Identity?.Name ?? string.Empty;
        var review = _xmlDataService.GetReviewById(id);
        
        if (review == null || review.UserName != userName)
        {
            return Forbid();
        }

        _xmlDataService.DeleteReview(id);
        return RedirectToPage();
    }
}
