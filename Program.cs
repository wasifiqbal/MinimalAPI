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


app.MapGet("/Info", () =>
{
    return "This is Demo Minimal API";
}).WithName("Information").Produces(200);

app.MapPost("/journals", async (JournalDbContext context, JournalEntry entry) =>
{
    context.JournalEntries.Add(entry);
    await context.SaveChangesAsync();
    return Results.Created($"/journal/{entry.Id}", entry);
}).Accepts(typeof(JournalEntry), "application/json").Produces(201).WithTags(new[] { "Add", "Journal" }); ;

app.MapGet("/journals", async (JournalDbContext context) => await context.JournalEntries.ToListAsync()).Produces(200).WithTags(new[] { "List", "Journal" }); ;

app.MapGet("/journals/{id}", async (JournalDbContext context, int id) =>
{
    return (await context.JournalEntries.FindAsync(id)) switch
    {
        null => Results.NotFound(),
        JournalEntry entry => Results.Ok(entry)

    };
}).Produces(200).ProducesProblem(404).WithTags(new[] { "Get", "Journal" }); ;

app.MapPut("/journals", async (JournalDbContext context, JournalEntry entry) =>
{
    var journalEntry = await context.JournalEntries.FindAsync(entry.Id);
    if (journalEntry is not null)
    {
        journalEntry.Title = entry.Title;
        journalEntry.Description = entry.Description;
        await context.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
}).Accepts<JournalEntry>("application/json").Produces(404).Produces(204).WithTags(new[] { "Update", "Journal" }); ;

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
}).Produces(204).ProducesProblem(404).WithTags(new[] { "Delete", "Journal" });

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