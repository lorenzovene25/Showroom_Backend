using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services.Interfaces;

public interface ICartService
{
    Task<CartItemDto?> AddItemAsync(AddCartItemDto dto, int userId, string culture = "en");
    Task<bool> CheckoutAsync(int userId, bool isPaymentSuccessful);
    Task ClearAsync(int cartId);
    Task<int> CreateCartAsync();
    Task<CartDto?> GetByIdAsync(int cartId, string culture = "en");
    Task<CartDto?> GetByUserAsync(int userId, string culture = "en");
    Task<bool> RemoveItemAsync(int cartId, int souvenirId);
    Task<CartItemDto?> UpdateItemQuantityAsync(int cartId, int souvenirId, PatchCartItemDto dto, string culture = "en");
}