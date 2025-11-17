using PsP.Models;

namespace PsP.Services
{
    public interface IGiftCardService
    {
        Task<GiftCard?> GetByIdAsync(int id);              
        Task<GiftCard?> ValidateAsync(string code, int? businessId = null);
        Task<GiftCard> CreateAsync(GiftCard card);
        Task<bool> TopUpAsync(int id, long amount);
        Task<(long charged, long remaining)> RedeemAsync(string code, long amount, int? businessId = null);
    }
}