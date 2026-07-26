using Microsoft.EntityFrameworkCore;
using MiniValidation;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

// ВИПРАВЛЕННЯ 1: секретний ключ більше не захардкоджений — читається з конфігурації
// (appsettings.json для dev, User Secrets/env variable для прод — див. коментар в кінці файлу)
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key не задано в конфігурації");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=students.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exception = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Внутрішня помилка сервера",
            detail = app.Environment.IsDevelopment() ? exception?.Message : null
        });
    });
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


app.MapGet("/students", async (IStudentService service) =>
{
    return await service.GetAllAsync();
}).RequireAuthorization();

app.MapGet("/students/{id}", async (int id, IStudentService service) =>
{
    var student = await service.GetByIdAsync(id);
    return student is null ? Results.NotFound() : Results.Ok(student);
}).RequireAuthorization();


app.MapPost("/students", async (CreateStudentDto input, IStudentService service) =>
{
    if (!MiniValidator.TryValidate(input, out var errors))
        return Results.BadRequest(errors);

    var student = await service.CreateAsync(input);
    return Results.Created($"/students/{student.Id}", student);
}).RequireAuthorization();

app.MapPut("/students/{id}", async (int id, UpdateStudentDto input, IStudentService service) =>
{
    if (!MiniValidator.TryValidate(input, out var errors))
        return Results.BadRequest(errors);

    var updated = await service.UpdateAsync(id, input);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
}).RequireAuthorization();

app.MapDelete("/students/{id}", async (int id, IStudentService service) =>
{
    var deleted = await service.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound();
}).RequireAuthorization();


app.MapPost("/register", async (LoginDto input, AppDbContext db) =>
{
    if (db.Users.Any(u => u.Email == input.Email))
        return Results.BadRequest(new { error = "Email вже існує" });

    var user = new User
    {
        Email = input.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(input.Password)
    };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Ok(new { message = "Реєстрація успішна" });
});

app.MapPost("/login", async (LoginDto input, AppDbContext db) =>
{
    var user = db.Users.FirstOrDefault(u => u.Email == input.Email);
    if (user is null || !BCrypt.Net.BCrypt.Verify(input.Password, user.PasswordHash))
        return Results.Unauthorized();

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds);

    return Results.Ok(new TokenDto(new JwtSecurityTokenHandler().WriteToken(token)));
});

app.Run();


public partial class Program { }