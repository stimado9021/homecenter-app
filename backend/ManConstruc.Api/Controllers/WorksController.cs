using System.Security.Claims;
using ManConstruc.Api.Contracts;
using ManConstruc.Api.Data;
using ManConstruc.Api.Models;
using ManConstruc.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ManConstruc.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/works")]
public class WorksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly DeepseekPricingService _aiPricing;

    public WorksController(AppDbContext db, DeepseekPricingService aiPricing)
    {
        _db = db;
        _aiPricing = aiPricing;
    }

    private Guid? CurrentUserId
    {
        get
        {
            var raw = User.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id) ? id : null;
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<WorkSummaryDto>>> GetAll()
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        var works = await _db.Works
            .AsNoTracking()
            .Where(work => work.UserId == userId)
            .Include(work => work.Items)
            .OrderByDescending(work => work.UpdatedAt)
            .ToListAsync();

        return Ok(works.Select(WorkMapper.ToSummary).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkDetailDto>> GetById(Guid id)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        var work = await QueryWithDetails()
            .FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.UserId == userId);

        return work is null
            ? NotFound(new { message = "La obra no existe." })
            : Ok(WorkMapper.ToDetail(work));
    }

    [HttpPost]
    public async Task<ActionResult<WorkDetailDto>> Create(WorkInput input)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        if (ValidateStructure(input) is { } error)
        {
            return BadRequest(new { message = error });
        }

        var work = WorkMapper.CreateEntity(input, userId);
        _db.Works.Add(work);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = work.Id }, WorkMapper.ToDetail(work));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WorkDetailDto>> Update(Guid id, WorkInput input)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        if (ValidateStructure(input) is { } error)
        {
            return BadRequest(new { message = error });
        }

        var work = await QueryWithDetails(tracking: true)
            .FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.UserId == userId);

        if (work is null)
        {
            return NotFound(new { message = "La obra no existe." });
        }

        var createdAt = work.CreatedAt;

        _db.Works.Remove(work);
        await _db.SaveChangesAsync();

        var newWork = WorkMapper.CreateEntity(input, userId);
        newWork.Id = id;
        newWork.CreatedAt = createdAt;
        _db.Works.Add(newWork);
        await _db.SaveChangesAsync();

        var saved = await QueryWithDetails()
            .FirstAsync(candidate => candidate.Id == id && candidate.UserId == userId);

        return Ok(WorkMapper.ToDetail(saved));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        var work = await _db.Works.FirstOrDefaultAsync(candidate => candidate.Id == id && candidate.UserId == userId);
        if (work is null)
        {
            return NotFound(new { message = "La obra no existe." });
        }

        _db.Works.Remove(work);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("preview")]
    public ActionResult<WorkCalculation> Preview(WorkInput input)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized(new { message = "Sesión no válida." });
        }

        if (ValidateStructure(input) is { } error)
        {
            return BadRequest(new { message = error });
        }

        var work = WorkMapper.CreateEntity(input, userId);
        return Ok(PricingCalculator.Calculate(work));
    }

    [HttpPost("estimate-prices")]
    public async Task<ActionResult<WorkInput>> EstimatePrices(WorkInput input, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized(new { message = "Sesi��n no vǭlida." });
        }

        if (ValidateStructure(input) is { } error)
        {
            return BadRequest(new { message = error });
        }

        try
        {
            var estimated = await _aiPricing.EstimatePricesAsync(input, cancellationToken);
            return Ok(estimated);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
        {
            return StatusCode(502, new { message = "La cuenta de Deepseek no tiene saldo suficiente. Recarga créditos en deepseek.com.", detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "No se pudo estimar precios con IA.", detail = ex.Message });
        }
    }

    [HttpPost("generate")]
    public async Task<ActionResult<WorkInput>> Generate(WorkGenerationRequest request, CancellationToken cancellationToken)
    {
        if (CurrentUserId is not { } userId)
        {
            return Unauthorized(new { message = "Sesi��n no vǭlida." });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new { message = "Describe el trabajo que quieres presupuestar." });
        }

        try
        {
            var generated = await _aiPricing.GenerateWorkFromDescriptionAsync(request.Description, cancellationToken);
            return Ok(generated);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PaymentRequired)
        {
            return StatusCode(502, new { message = "La cuenta de Deepseek no tiene saldo suficiente. Recarga créditos en deepseek.com.", detail = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { message = "No se pudo generar el presupuesto con IA.", detail = ex.Message });
        }
    }

    private IQueryable<Work> QueryWithDetails(bool tracking = false)
    {
        var query = _db.Works
            .Include(work => work.Items)
            .ThenInclude(item => item.Materials)
            .Include(work => work.Items)
            .ThenInclude(item => item.Labors)
            .AsQueryable();

        return tracking ? query : query.AsNoTracking();
    }

    private static string? ValidateStructure(WorkInput input)
    {
        for (var index = 0; index < input.Items.Count; index++)
        {
            var item = input.Items[index];
            if (item.Materials.Count == 0 && item.Labors.Count == 0)
            {
                return $"La partida #{index + 1} debe tener al menos un material o una actividad de mano de obra.";
            }
        }

        return null;
    }
}
