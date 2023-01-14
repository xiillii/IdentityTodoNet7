using IdentityTodo.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityTodo.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext ctx)
    {
        _logger = logger;
        _context = ctx;
    }

    [BindProperty(SupportsGet = true)] public bool ShowComplete { get; set; }

    public IEnumerable<TodoItem> TodoItems { get; set; }

    public void OnGet()
    {
        TodoItems = _context.TodoItems.Where(t => t.Owner == User.Identity.Name).OrderBy(t => t.Task);

        if (!ShowComplete)
        {
            TodoItems = TodoItems.Where(t => !t.Complete);
        }

        TodoItems = TodoItems.ToList();
    }

    public IActionResult OnPostShowComplete() => RedirectToPage(new { ShowComplete });

    public async Task<IActionResult> OnPostAddItemAsync(string task)
    {
        if (!string.IsNullOrEmpty(task))
        {
            var item = new TodoItem
            {
                Task = task,
                Owner = User.Identity.Name,
                Complete = false,
                Added = DateTimeOffset.Now
            };

            await _context.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage(new { ShowComplete });
    }

    public async Task<IActionResult> OnPostMarkItemAsync(long id)
    {
        var item = await _context.TodoItems.FindAsync(id);

        if (item != null)
        {
            item.Complete = !item.Complete;
            item.Completed = item.Complete ? DateTimeOffset.Now : null;
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(new { ShowComplete });
    }
}
