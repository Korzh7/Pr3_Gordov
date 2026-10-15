namespace Pr3_Gordov.API.Models;

public record OrderItemDto(long ClothingSizeId, int Quantity);

public record CreateOrderDto(List<OrderItemDto> Items);
