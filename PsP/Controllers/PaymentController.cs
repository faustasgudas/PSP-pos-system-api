using Microsoft.AspNetCore.Mvc;
using PsP.Services;

namespace PsP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly PaymentService _payments;

    public PaymentController(PaymentService payments)
    {
        _payments = payments;
    }

    [HttpPost("create")]
    public async Task<ActionResult> Create(
        long amountCents,
        string currency,
        int businessId,
        string? giftCardCode = null,
        long? giftCardAmountCents = null)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        var result = await _payments.CreatePaymentAsync(
            amountCents,
            currency,
            businessId,
            giftCardCode,
            giftCardAmountCents,
            baseUrl);

        return Ok(result);
    }

    [HttpGet("success")]
    public async Task<IActionResult> Success(string sessionId)
    {
        await _payments.ConfirmStripeSuccessAsync(sessionId);
        return Ok("Payment successful.");
    }

    [HttpGet("cancel")]
    public async Task<IActionResult> Cancel(string sessionId)
    {
        // čia gali pažymėti Payment kaip "Cancelled", jei nori
        return Ok("Payment cancelled.");
    }
}