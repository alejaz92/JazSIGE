﻿using FiscalDocumentationService.Business.Interfaces;
using FiscalDocumentationService.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FiscalDocumentationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FiscalDocumentController : ControllerBase
    {
        private readonly IFiscalDocumentService _fiscalDocumentService;

        public FiscalDocumentController(IFiscalDocumentService fiscalDocumentService)
        {
            _fiscalDocumentService = fiscalDocumentService;
        }

        [HttpPost]
        public async Task<ActionResult<FiscalDocumentDTO>> Create([FromBody] FiscalDocumentCreateDTO dto)
        {
            var result = await _fiscalDocumentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FiscalDocumentDTO>> GetById(int id)
        {
            var document = await _fiscalDocumentService.GetByIdAsync(id);
            if (document == null)
                return NotFound();

            return Ok(document);
        }

        [HttpGet("by-sales-order/{salesOrderId}")]
        public async Task<ActionResult<FiscalDocumentDTO>> GetBySalesOrderId(int salesOrderId)
        {
            var document = await _fiscalDocumentService.GetBySalesOrderIdAsync(salesOrderId);
            if (document == null)
                return NotFound();

            return Ok(document);
        }
    }
}
