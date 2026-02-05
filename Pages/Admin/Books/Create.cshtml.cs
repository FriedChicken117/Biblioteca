using System.ComponentModel.DataAnnotations;
using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Biblioteca.Pages.Admin.Books;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly XmlDataService _xmlDataService;

    public CreateModel(XmlDataService xmlDataService)
    {
        _xmlDataService = xmlDataService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Título")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Autor")]
        public string Author { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Categoría")]
        public string Category { get; set; } = string.Empty;

        [Display(Name = "Resumen")]
        public string Summary { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var book = new Book
        {
            Title = Input.Title,
            Author = Input.Author,
            Category = Input.Category,
            Summary = Input.Summary
        };

        _xmlDataService.AddBook(book);

        return RedirectToPage("/Books/Details", new { id = book.Id });
    }
}

