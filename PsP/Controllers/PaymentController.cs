using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PsP.Data;
using PsP.Models;
using PsP.Services;

namespace PsP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly StripePaymentService _paymentService;
    private readonly AppDbContext _context;

    public PaymentController(StripePaymentService paymentService, AppDbContext context)
    {
        _paymentService = paymentService;
        _context = context;
    }

    [HttpPost("create-checkout-session")]
    public ActionResult CreateCheckoutSession()
    {
        var session = _paymentService.CreateCheckoutSession(
            "https://yourdomain.com/payment/success?sessionId={CHECKOUT_SESSION_ID}",
            "https://yourdomain.com/payment/cancel?sessionId={CHECKOUT_SESSION_ID}"
        );

        _context.PaymentRecords.Add(new PaymentRecord
        {
            StripeSessionId = session.Id,
            Created = DateTime.UtcNow,
            Status = "Created"
        });
        _context.SaveChanges();

        return Ok(new { sessionId = session.Id, url = session.Url });
    }

    [HttpGet("success")]
    public async Task<IActionResult> Success(string sessionId)
    {
        var paymentRecord = await _context.PaymentRecords
            .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId);

        if (paymentRecord != null)
        {
            paymentRecord.Status = "Success";
            await _context.SaveChangesAsync();
        }

        return Ok("Payment successful.");
    }

    [HttpGet("cancel")]
    public async Task<IActionResult> Cancel(string sessionId)
    {
        var paymentRecord = await _context.PaymentRecords
            .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId);

        if (paymentRecord != null)
        {
            paymentRecord.Status = "Cancelled";
            await _context.SaveChangesAsync();
        }

        return Ok("Payment cancelled.");
    }
}
