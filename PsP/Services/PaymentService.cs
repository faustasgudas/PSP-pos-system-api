using PsP.Data;
using PsP.Models;
using Microsoft.EntityFrameworkCore;

namespace PsP.Services;

public class PaymentService
{
    private readonly AppDbContext _db;
    private readonly IGiftCardService _giftCards;
    private readonly StripePaymentService _stripe;

    public PaymentService(AppDbContext db, IGiftCardService giftCards, StripePaymentService stripe)
    {
        _db = db;
        _giftCards = giftCards;
        _stripe = stripe;
    }

    public async Task<PaymentResult> CreatePaymentAsync(
        long amountCents,
        string currency,
        int businessId,
        string? giftCardCode,
        long? giftCardAmountCents,
        string baseUrl)
    {
        GiftCard? card = null;
        long plannedFromGiftCard = 0;
        long remainingForStripe = amountCents;

        if (!string.IsNullOrWhiteSpace(giftCardCode))
        {
            card = await _giftCards.ValidateAsync(giftCardCode, businessId)
                   ?? throw new Exception("invalid_gift_card");

            var maxFromCard = Math.Min(card.Balance, amountCents);

            if (giftCardAmountCents.HasValue)
            {
                if (giftCardAmountCents.Value <= 0)
                    throw new ArgumentOutOfRangeException(nameof(giftCardAmountCents));

                plannedFromGiftCard = Math.Min(giftCardAmountCents.Value, maxFromCard);
            }
            else
            {
                plannedFromGiftCard = maxFromCard; // senas elgesys: naudoti max
            }

            remainingForStripe = amountCents - plannedFromGiftCard;
        }

        var p = new Payment
        {
            AmountCents          = amountCents,
            Currency             = currency,
            CreatedAt            = DateTime.UtcNow,
            Status               = "Pending",
            Method               = remainingForStripe == 0 ? "GiftCard" : "GiftCard+Stripe",
            GiftCardId           = card?.GiftCardId,
            BusinessId           = businessId,
            GiftCardPlannedCents = plannedFromGiftCard
        };

        _db.Payments.Add(p);
        await _db.SaveChangesAsync();

        string? stripeUrl = null;
        string? stripeSessionId = null;

        if (remainingForStripe > 0)
        {
            // DABAR baseUrl gaunam iš controllerio, o ne "https://yourdomain.lt"
            var successUrl = $"{baseUrl}/api/payment/success?sessionId={{CHECKOUT_SESSION_ID}}";
            var cancelUrl  = $"{baseUrl}/api/payment/cancel?sessionId={{CHECKOUT_SESSION_ID}}";

            var session = _stripe.CreateCheckoutSession(
                remainingForStripe,
                currency,
                successUrl,
                cancelUrl,
                p.PaymentId
            );

            stripeUrl = session.Url;
            stripeSessionId = session.Id;

            p.StripeSessionId = session.Id;
            await _db.SaveChangesAsync();
        }
        else
        {
            // 100% gift card apmokėjimas – čia jau galima iškart nurašyti
            if (card is not null && plannedFromGiftCard > 0)
            {
                await _giftCards.RedeemAsync(card.Code, plannedFromGiftCard, businessId);
            }

            p.Status = "Success";
            p.CompletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return new PaymentResult(
            p.PaymentId,
            plannedFromGiftCard,
            remainingForStripe,
            stripeUrl,
            stripeSessionId
        );
    }

    // kviečiama iš /api/payment/success
    public async Task ConfirmStripeSuccessAsync(string sessionId)
    {
        var p = await _db.Payments
            .Include(x => x.GiftCard)
            .FirstOrDefaultAsync(x => x.StripeSessionId == sessionId);

        if (p == null) return;
        if (p.Status == "Success") return; // idempotency

        if (p.GiftCardId != null &&
            p.GiftCardPlannedCents > 0 &&
            p.GiftCard is not null)
        {
            await _giftCards.RedeemAsync(
                p.GiftCard.Code,
                p.GiftCardPlannedCents,
                p.BusinessId);
        }

        p.Status = "Success";
        p.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}

public record PaymentResult(
    int PaymentId,
    long PaidByGiftCard,
    long RemainingForStripe,
    string? StripeUrl,
    string? StripeSessionId
);
