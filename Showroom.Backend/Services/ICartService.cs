using Showroom.Backend.Dtos;

namespace Showroom.Backend.Services;

public interface ICartService
{
    Task<CartItemDto?> AddItemAsync(int cartId, AddCartItemDto dto, string culture = "en");
    Task ClearAsync(int cartId);
    Task<int> CreateCartAsync();
    Task<CartDto?> GetByIdAsync(int cartId, string culture = "en");
    Task<CartDto?> GetByUserAsync(int userId, string culture = "en");
    Task<bool> RemoveItemAsync(int cartId, int souvenirId);
    Task<CartItemDto?> UpdateItemQuantityAsync(int cartId, int souvenirId, PatchCartItemDto dto, string culture = "en");
}