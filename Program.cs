using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
IConfiguration configuration = builder.Configuration;
// Add services to the container.
builder.Services.AddDbContext<JournalDbContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("Default"));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/ping", () =>
{
    return "pong";
});

app.MapPost("/journals", async (JournalDbContext context, JournalEntry entry) =>
{
    context.JournalEntries.Add(entry);
    await context.SaveChangesAsync();
    return Results.Created($"/journal/{entry.Id}", entry);
});

app.MapGet("/journals", async (JournalDbContext context) =>
{
    return await context.JournalEntries.ToListAsync();
});

app.MapGet("/journals/{id}", async (JournalDbContext context, int id) =>
{
    var entry = await context.JournalEntries.FindAsync(id);
    if (entry is not null)
        return Results.Ok(entry);
    return Results.NotFound();
});

app.MapPut("/journals/{id}", async (JournalDbContext context, int id, JournalEntry entry) =>
{
    var journalEntry = await context.JournalEntries.FindAsync(id);
    if (journalEntry is not null)
    {
        journalEntry.Title = entry.Title;
        journalEntry.Description = entry.Description;
        await context.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.MapDelete("/journals/{id}", async (JournalDbContext context, int id) =>
{
    var journalEntry = await context.JournalEntries.FindAsync(id);
    if (journalEntry is not null)
    {
        context.JournalEntries.Remove(journalEntry);
        await context.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});

app.Run();


public class JournalDbContext : DbContext
{
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public JournalDbContext() { }
    public JournalDbContext(DbContextOptions options) : base(options) { }
}


public class JournalEntry
{
    public int Id { get; set; }
    [Required]
    public string Title { get; set; } = String.Empty;
    public string Description { get; set; } = String.Empty;
    public DateTime Created { get; set; } = DateTime.UtcNow;
}