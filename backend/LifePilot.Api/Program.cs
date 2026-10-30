using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5080");
builder.Services.AddDbContext<LifeDb>(options => options.UseSqlite("Data Source=lifepilot.db"));
builder.Services.AddCors(options => options.AddPolicy("local", p => p.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173").AllowAnyHeader().AllowAnyMethod()));
var app = builder.Build();
app.UseCors("local");
using (var scope = app.Services.CreateScope()) scope.ServiceProvider.GetRequiredService<LifeDb>().Database.EnsureCreated();
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapGet("/api/items", async (LifeDb db, string? kind) => await db.Items.Where(x => kind == null || x.Kind == kind).OrderByDescending(x => x.Id).ToListAsync());
app.MapPost("/api/items", async (LifeDb db, NewItem input) => {
    if (!ValidKind(input.Kind) || string.IsNullOrWhiteSpace(input.Title) || input.Title.Length > 200) return Results.BadRequest(new { error = "Valid kind and title required (max 200 chars)." });
    if (input.Amount is < 0 or > 100000000) return Results.BadRequest(new { error = "Amount must be between 0 and 100000000." });
    if (input.Date is not null && !DateOnly.TryParse(input.Date, out _)) return Results.BadRequest(new { error = "Date must be YYYY-MM-DD." });
    var item = new LifeItem { Kind = input.Kind, Title = input.Title.Trim(), Category = input.Category?.Trim() ?? "", Amount = input.Amount ?? 0, Date = input.Date, Done = false, CreatedUtc = DateTime.UtcNow };
    db.Items.Add(item); await db.SaveChangesAsync(); return Results.Created($"/api/items/{item.Id}", item);
});
app.MapPut("/api/items/{id:int}", async (int id, UpdateItem input, LifeDb db) => {
    var item = await db.Items.FindAsync(id); if (item is null) return Results.NotFound();
    if (input.Title is not null) { if (string.IsNullOrWhiteSpace(input.Title) || input.Title.Length > 200) return Results.BadRequest(); item.Title = input.Title.Trim(); }
    if (input.Category is not null) item.Category = input.Category.Trim();
    if (input.Amount is not null) { if (input.Amount is < 0 or > 100000000) return Results.BadRequest(); item.Amount = input.Amount.Value; }
    if (input.Date is not null) { if (input.Date != "" && !DateOnly.TryParse(input.Date, out _)) return Results.BadRequest(); item.Date = input.Date == "" ? null : input.Date; }
    if (input.Done is not null) item.Done = input.Done.Value;
    await db.SaveChangesAsync(); return Results.Ok(item);
});
app.MapDelete("/api/items/{id:int}", async (int id, LifeDb db) => { var item = await db.Items.FindAsync(id); if (item is null) return Results.NotFound(); db.Items.Remove(item); await db.SaveChangesAsync(); return Results.NoContent(); });
app.MapGet("/api/backup", async (LifeDb db) => Results.Json(await db.Items.OrderBy(x => x.Id).ToListAsync()));
app.MapPost("/api/restore", async (LifeDb db, List<NewItem> items) => {
    if (items.Count > 5000 || items.Any(x => !ValidKind(x.Kind) || string.IsNullOrWhiteSpace(x.Title) || x.Title.Length > 200 || x.Amount is < 0 or > 100000000 || (x.Date is not null && !DateOnly.TryParse(x.Date, out _)))) return Results.BadRequest(new { error = "Invalid backup" });
    db.Items.AddRange(items.Select(x => new LifeItem { Kind=x.Kind, Title=x.Title.Trim(), Category=x.Category ?? "", Amount=x.Amount ?? 0, Date=x.Date, CreatedUtc=DateTime.UtcNow }));
    await db.SaveChangesAsync(); return Results.Ok(new { imported = items.Count });
});
app.MapPost("/api/assistant", (Command input) => {
    var s = input.Text.Trim();
    var grocery = Regex.Match(s, @"^(?:add|buy)\s+(.+?)\s+(?:to\s+(?:my\s+)?(?:grocery|groceries|shopping)(?:\s+list)?)$", RegexOptions.IgnoreCase);
    if (grocery.Success) return Results.Ok(new { kind="grocery", title=grocery.Groups[1].Value, message="Ready to add to groceries." });
    var remind = Regex.Match(s, @"^remind me to\s+(.+)$", RegexOptions.IgnoreCase);
    if (remind.Success) return Results.Ok(new { kind="task", title=remind.Groups[1].Value, message="Ready to add a task. Set a date if needed." });
    var spend = Regex.Match(s, @"^(?:spent|spend)\s+\$?([0-9]+(?:\.[0-9]{1,2})?)\s+(?:on|for)\s+(.+)$", RegexOptions.IgnoreCase);
    if (spend.Success) return Results.Ok(new { kind="expense", title=spend.Groups[2].Value, amount=decimal.Parse(spend.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture), message="Ready to record expense." });
    return Results.Ok(new { kind="", title="", message="Try: add milk to groceries; remind me to call mom; spent 12.50 on lunch" });
});
app.Run();
static bool ValidKind(string kind) => kind is "task" or "expense" or "grocery" or "event" or "document";
public class LifeDb(DbContextOptions<LifeDb> options) : DbContext(options) { public DbSet<LifeItem> Items => Set<LifeItem>(); }
public class LifeItem { public int Id {get;set;} public string Kind {get;set;} = ""; public string Title {get;set;} = ""; public string Category {get;set;} = ""; public decimal Amount {get;set;} public string? Date {get;set;} public bool Done {get;set;} public DateTime CreatedUtc {get;set;} }
public record NewItem(string Kind, string Title, string? Category, decimal? Amount, string? Date);
public record UpdateItem(string? Title, string? Category, decimal? Amount, string? Date, bool? Done);
public record Command(string Text);
