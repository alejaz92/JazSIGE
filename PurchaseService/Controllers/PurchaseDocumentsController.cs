using Microsoft.AspNetCore.Mvc;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;
using PurchaseService.Business.Exceptions;
using PurchaseService.Business.Models.Clients;

namespace PurchaseService.Controllers;

[ApiController]
[Route("api/{purchaseId:int}/documents")]
public class PurchaseDocumentsController : ControllerBase
{
    private readonly IPurchaseDocumentService _service;

    public PurchaseDocumentsController(IPurchaseDocumentService service)
    {
        _service = service;
    }

    // GET api/purchases/{purchaseId}/documents?onlyActive=true
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseDocumentDTO>>> GetByPurchaseId(
        [FromRoute] int purchaseId,
        [FromQuery] bool onlyActive = false)
    {
        try
        {
            var docs = await _service.GetByPurchaseIdAsync(purchaseId, onlyActive);
            return Ok(docs);
        }
        catch (DomainException ex) when (ex.Code == "PURCHASE_NOT_FOUND")
        {
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    // POST api/purchases/{purchaseId}/documents
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromRoute] int purchaseId,
        [FromBody] PurchaseDocumentCreateDTO dto)
    {
        // Igual que en tus controladores: se toma el userId de X-UserId
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userIdHeader, out int userId))
            return Unauthorized(new { error = "Usuario no autenticado correctamente" });

        try
        {
            var created = await _service.CreateAsync(purchaseId, dto, userId);
            // Podés devolver Created con location si querés; como no tenemos GET by id, devuelvo el objeto
            return Created($"/api/purchases/{purchaseId}/documents", created);
        }
        catch (DomainException ex) when (
            ex.Code == "PURCHASE_NOT_FOUND" ||
            ex.Code == "INVOICE_ALREADY_EXISTS" ||
            ex.Code == "INVOICE_REQUIRED_FOR_NOTES" ||
            ex.Code == "DOCUMENT_NUMBER_DUPLICATED")
        {
            // 404 para purchase not found, 400 para reglas de negocio
            var status = ex.Code == "PURCHASE_NOT_FOUND" ? 404 : 400;
            return StatusCode(status, new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }

    // POST api/purchases/{purchaseId}/documents/{documentId}/cancel
    [HttpPost("{documentId:int}/cancel")]
    public async Task<IActionResult> Cancel(
        [FromRoute] int purchaseId,
        [FromRoute] int documentId,
        [FromBody] PurchaseDocumentCancelDTO dto)
    {
        var userIdHeader = HttpContext.Request.Headers["X-UserId"].ToString();
        if (!int.TryParse(userIdHeader, out int userId))
            return Unauthorized(new { error = "Usuario no autenticado correctamente" });

        try
        {
            await _service.CancelAsync(purchaseId, documentId, dto, userId);
            return Ok();
        }
        catch (DomainException ex) when (
            ex.Code == "PURCHASE_NOT_FOUND" ||
            ex.Code == "DOCUMENT_NOT_FOUND" ||
            ex.Code == "CANCEL_REASON_REQUIRED")
        {
            var status = ex.Code switch
            {
                "PURCHASE_NOT_FOUND" => 404,
                "DOCUMENT_NOT_FOUND" => 404,
                "CANCEL_REASON_REQUIRED" => 400,
                _ => 400
            };
            return StatusCode(status, new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }


    // endpoint to cover using CoverInvoiceWithReceiptsAsync from _service

    // POST api/{purchaseId}/documents/cover-with-receipts
    [HttpPost("cover")]
    public async Task<IActionResult> CoverInvoiceWithReceipts(
        [FromRoute] int purchaseId,
        [FromBody] CoverInvoiceRequest request,
        CancellationToken ct = default)
    {
        if (request == null)
            return BadRequest(new { error = "Request body is required" });

        try
        {
            await _service.CoverInvoiceWithReceiptsAsync(purchaseId, request, ct);
            return Ok();
        }
        catch (DomainException ex) when (ex.Code == "PURCHASE_NOT_FOUND")
        {
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (DomainException ex)
        {
            // Reglas de negocio distintas devuelven 400
            return BadRequest(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Unexpected error", detail = ex.Message });
        }
    }



}
