using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OnCourse.Turnstile.RazorPages.Pages;

[ValidateTurnstile]
public class IndexModel : PageModel
{
    [BindProperty]
    public string Email { get; set; } = "";

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        TempData["Message"] = $"Email {Email} submitted successfully!";
        Email = string.Empty;

        return RedirectToPage();
    }
}
