using PsP.Data;
using PsP.Models;
using Microsoft.EntityFrameworkCore;

namespace PsP.Services
{
    public class GiftCardService : IGiftCardService
    {
        private readonly AppDbContext _db;
        public GiftCardService(AppDbContext db) => _db = db;

        public Task<GiftCard?> GetByIdAsync(int id)
            => _db.GiftCards.FirstOrDefaultAsync(x => x.GiftCardId == id);

        public async Task<GiftCard?> ValidateAsync(string code, int? businessId = null)
        {
            var c = await _db.GiftCards.FirstOrDefaultAsync(x => x.Code == code);
            if (c is null) return null;

            if (businessId.HasValue && c.BusinessId != businessId.Value)
                throw new InvalidOperationException("wrong_business");
            if (c.Status != "Active")
                throw new InvalidOperationException("blocked");
            if (c.ExpiresAt is not null && c.ExpiresAt <= DateTime.UtcNow)
                throw new InvalidOperationException("expired");

            return c;
        }

        public async Task<GiftCard> CreateAsync(GiftCard card)
        {
            card.IssuedAt = DateTime.UtcNow;
            card.Status ??= "Active";
            _db.GiftCards.Add(card);
            await _db.SaveChangesAsync();
            return card;
        }

        public async Task<bool> TopUpAsync(int id, long amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

            var c = await _db.GiftCards.FindAsync(id);
            if (c is null) return false;
            if (c.Status != "Active") throw new InvalidOperationException("blocked");
            if (c.ExpiresAt is not null && c.ExpiresAt <= DateTime.UtcNow)
                throw new InvalidOperationException("expired");

            c.Balance += amount;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<(long charged, long remaining)> RedeemAsync(
            string code,
            long amount,
            int? businessId = null)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));

            var c = await ValidateAsync(code, businessId)
                    ?? throw new KeyNotFoundException("not_found");

            var charge = Math.Min(c.Balance, amount);
            if (charge == 0)
                return (0, c.Balance);

            c.Balance -= charge;
            await _db.SaveChangesAsync();

            return (charge, c.Balance);
        }
    }
}
