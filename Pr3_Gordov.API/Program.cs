using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pr3_Gordov.API.Data;
using Pr3_Gordov.API.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});


builder.Services.AddDbContext<Pr3_GordovContext>();

var key = new SymmetricSecurityKey(
    RandomNumberGenerator.GetBytes(32));

var validation = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "JwtDemo",

    ValidateAudience = true,
    ValidAudience = "JwtDemoApi",

    ValidateIssuerSigningKey = true,
    IssuerSigningKey = key,

    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero,

    NameClaimType = "name",
    RoleClaimType = "role"
};

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = validation;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

var hasher = new PasswordHasher<User>();


app.MapPost("/api/auth/login", async (LoginRequest request, Pr3_GordovContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Login == request.Login);

    if (user == null || string.IsNullOrWhiteSpace(request.Password))
        return Results.Challenge();

    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

    if (result == PasswordVerificationResult.Failed)
        return Results.Challenge();

    return Results.Ok(new
    {
        access_token = CreateToken(user),
        token_type = "Bearer"
    });
}).AllowAnonymous();

app.MapPost("/api/auth/register", async (RegisterRequest request, Pr3_GordovContext db) =>
{
    if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest("Логин и пароль обязательны");

    if (await db.Users.AnyAsync(u => u.Login == request.Login))
        return Results.BadRequest("Пользователь с таким логином уже существует");

    var user = new User
    {
        Login = request.Login,
        Phone = request.Phone ?? "",
        DeliveryAddress = request.DeliveryAddress ?? "",
        Role = "user",
        CreatedAt = DateTime.UtcNow
    };
    user.PasswordHash = hasher.HashPassword(user, request.Password);

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        access_token = CreateToken(user),
        token_type = "Bearer"
    });
}).AllowAnonymous();

app.MapGet("/api/me", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        id = user.FindFirst("sub")?.Value,
        login = user.Identity?.Name,
        role = user.FindFirst("role")?.Value
    });
}).RequireAuthorization();


app.MapGet("/api/clothing", async (Pr3_GordovContext context) =>
{
    var clothes = await context.Clothings
        .Where(c => c.IsAvailable)
        .Include(c => c.ClothingSizes)
        .ToListAsync();

    return Results.Ok(clothes);
});

app.MapGet("/api/clothing/search", async (Pr3_GordovContext context, string title) =>
{
    var clothes = await context.Clothings
        .Where(c => c.IsAvailable && c.Title.Contains(title))
        .Include(c => c.ClothingSizes)
        .ToListAsync();

    return Results.Ok(clothes);
});


app.MapPost("/api/orders", async (CreateOrderDto request, ClaimsPrincipal user, Pr3_GordovContext context) =>
{
    var userIdString = user.FindFirst("sub")?.Value;
    if (userIdString == null) return Results.Unauthorized();
    long userId = long.Parse(userIdString);

    if (request.Items == null || !request.Items.Any())
        return Results.BadRequest("Заказ должен содержать хотя бы один товар");

    var order = new Order
    {
        UserId = userId,
        Status = "created",
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    foreach (var item in request.Items)
    {
        var size = await context.ClothingSizes
            .Include(s => s.Clothing)
            .FirstOrDefaultAsync(s => s.Id == item.ClothingSizeId);

        if (size == null)
            return Results.BadRequest($"Размер с ID {item.ClothingSizeId} не найден");

        if (size.StockQuantity < item.Quantity)
            return Results.BadRequest($"Недостаточно остатков для товара \"{size.Clothing.Title}\" ({size.LetterSize}). В наличии: {size.StockQuantity}");

        
        size.StockQuantity -= item.Quantity;
        size.Clothing.PurchasesCount += item.Quantity;

        order.OrderItems.Add(new OrderItem
        {
            ClothingSizeId = item.ClothingSizeId,
            Quantity = item.Quantity,
            UnitPrice = size.Clothing.Price
        });
    }

    context.Orders.Add(order);
    await context.SaveChangesAsync();

    return Results.Ok(new
    {
        orderId = order.Id,
        status = order.Status,
        message = "Заказ успешно оформлен"
    });
}).RequireAuthorization();

app.MapGet("/api/orders/status/{status}", async (Pr3_GordovContext context, string status) =>
{
    var orders = await context.Orders
        .Where(o => o.Status == status)
        .Include(o => o.OrderItems)
        .ToListAsync();

    return Results.Ok(orders);
}).RequireAuthorization();

app.MapGet("/api/orders/user/{userId}", async (Pr3_GordovContext context, long userId) =>
{
    var orders = await context.Orders
        .Where(o => o.UserId == userId)
        .Include(o => o.OrderItems)
        .ToListAsync();

    return Results.Ok(orders);
}).RequireAuthorization();

app.MapPut("/api/orders/{id}/status", async (Pr3_GordovContext context, long id, string status) =>
{
    var order = await context.Orders.FindAsync(id);
    if (order == null)
        return Results.NotFound("Заказ не найден");

    order.Status = status;
    order.UpdatedAt = DateTime.UtcNow;
    await context.SaveChangesAsync();

    return Results.Ok(order);
}).RequireAuthorization(p => p.RequireRole("manager"));

app.MapPut("/api/clothing/sizes/{id}/stock", async (Pr3_GordovContext context, long id, int quantity) =>
{
    var size = await context.ClothingSizes.FindAsync(id);
    if (size == null)
        return Results.NotFound("Размер товара не найден");

    size.StockQuantity = quantity;
    await context.SaveChangesAsync();

    return Results.Ok(size);
}).RequireAuthorization(p => p.RequireRole("manager"));

app.MapPut("/api/clothing/{id}/disable", async (Pr3_GordovContext context, long id) =>
{
    var cloth = await context.Clothings.FindAsync(id);
    if (cloth == null)
        return Results.NotFound("Товар не найден");

    cloth.IsAvailable = false;
    await context.SaveChangesAsync();

    return Results.Ok(cloth);
}).RequireAuthorization(p => p.RequireRole("manager"));

app.MapPut("/api/clothing/{id}/enable", async (Pr3_GordovContext context, long id) =>
{
    var cloth = await context.Clothings.FindAsync(id);
    if (cloth == null)
        return Results.NotFound("Товар не найден");

    cloth.IsAvailable = true;
    await context.SaveChangesAsync();

    return Results.Ok(cloth);
}).RequireAuthorization(p => p.RequireRole("manager"));

app.Run();


string CreateToken(User user)
{
    var claims = new[]
    {
        new Claim("sub", user.Id.ToString()),
        new Claim("name", user.Login),
        new Claim("role", user.Role)
    };

    var token = new JwtSecurityToken(
        issuer: "JwtDemo",
        audience: "JwtDemoApi",
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256));

    return new JwtSecurityTokenHandler().WriteToken(token);
}
