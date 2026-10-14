namespace Pr3_Gordov.API.Models;

public record RegisterRequest(
    string Login,
    string Password,
    string? Phone,
    string? DeliveryAddress);
