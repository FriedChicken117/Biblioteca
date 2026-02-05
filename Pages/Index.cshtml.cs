using Biblioteca.Models;
using Biblioteca.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Biblioteca.Pages;

public class IndexModel : PageModel
{
    private readonly XmlDataService _xmlDataService;

    public IndexModel(XmlDataService xmlDataService)
    {
        _xmlDataService = xmlDataService;
    }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Author { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Category { get; set; }

    public IEnumerable<Book> Books { get; set; } = Enumerable.Empty<Book>();
    public IEnumerable<string> Categories { get; set; } = Enumerable.Empty<string>();

    public void OnGet()
    {
        Books = _xmlDataService.GetBooks(Search, Author, Category);
        Categories = _xmlDataService.GetCategories();
    }
}

