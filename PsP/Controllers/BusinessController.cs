using Microsoft.AspNetCore.Mvc;
using PsP.Services;
using PsP.Models;

namespace PsP.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BusinessController : ControllerBase
{
    private readonly IBusinessService _businessService;

    public BusinessController(IBusinessService businessService)
    {
        _businessService = businessService;
    }

    [HttpGet]
    public async Task<ActionResult<List<Business>>> GetAll()
    {
        var list = await _businessService.GetAllAsync();
        return Ok(list);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Business>> GetById(int id)
    {
        var business = await _businessService.GetByIdAsync(id);
        if (business == null)
            return NotFound();

        return Ok(business);
    }

    [HttpPost]
    public async Task<ActionResult<Business>> Create([FromBody] Business business)
    {
        var created = await _businessService.CreateAsync(business);
        return CreatedAtAction(nameof(GetById),
            new { id = created.BusinessId },
            created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Business>> Update(int id, [FromBody] Business business)
    {
        var updated = await _businessService.UpdateAsync(id, business);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _businessService.DeleteAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}