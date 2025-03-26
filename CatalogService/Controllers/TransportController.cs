using CatalogService.Business.Interfaces;
using CatalogService.Business.Models.Transport;
using CatalogService.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransportController : BaseController<Transport, TransportDTO, TransportCreateDTO>
    {
        private readonly ITransportService _transportService;

        public TransportController(ITransportService transportService) : base(transportService)
        {
            _transportService = transportService;
        }
    }
}
