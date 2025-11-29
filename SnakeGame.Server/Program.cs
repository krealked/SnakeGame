using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SnakeGame.Server.Data;
using SnakeGame.Server.Middlewares;
using SnakeGame.Server.Services;
var builder = WebApplication.CreateBuilder(args);
// Add services
builder.Services.AddControllers();
// DbContext - SQLite
var connectionString =
builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=snakegame.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
// DI
builder.Services.AddScoped<IGameService, GameService>();
// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
c.SwaggerDoc("v1", new OpenApiInfo
{
    Title = "SnakeGame API",
    Version =
"v1"
});
});
var app = builder.Build();
// Ensure DB created & apply migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
// Middlewares
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json",
    "SnakeGame API v1"));
}
app.UseRouting();
app.MapControllers();
app.Run();