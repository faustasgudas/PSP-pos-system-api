using Microsoft.Extensions.Options;
using PsP.Settings;
using Stripe;
using Stripe.Checkout;

namespace PsP.Services;

public class StripePaymentService
{
    private readonly StripeSettings _settings;

    public StripePaymentService(IOptions<StripeSettings> stripeSettings)
    {
        _settings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public Session CreateCheckoutSession(string successUrl, string cancelUrl)
    {
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = 2000, // 20.00 (centais)
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "T-shirt",
                        },
                    }
                },
            }
        };

        var service = new SessionService();
        return service.Create(options);
    }
}