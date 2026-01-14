using Moq;
using SalesService.Business.Services;
using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.Sale;
using SalesService.Business.Models.DeliveryNote;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SalesService.Tests
{
 public class InvoiceCreationTests
 {
 [Fact]
 public async Task CreateInvoice_CallsFiscalService_WithCorrectTotals()
 {
 // Arrange
 var unitOfWorkMock = new Mock<IUnitOfWork>();
 var catalogClientMock = new Mock<ICatalogServiceClient>();
 var stockClientMock = new Mock<IStockServiceClient>();
 var userClientMock = new Mock<IUserServiceClient>();
 var fiscalClientMock = new Mock<IFiscalServiceClient>();
 var accountingClientMock = new Mock<IAccountingServiceClient>();
 var deliveryNoteServiceMock = new Mock<IDeliveryNoteService>();
 var companyClientMock = new Mock<ICompanyServiceClient>();

 var sale = new Sale
 {
 Id =1,
 IsFinalConsumer = false,
 CustomerId =10,
 CustomerIdType = "CUIT",
 CustomerTaxId = "20123456789",
 CustomerName = "ACME SRL",
 SellerId =2,
 ExchangeRate =1,
 HasInvoice = false,
 IsFullyDelivered = true,
 Articles = new List<Sale_Article>
 {
 new Sale_Article { ArticleId =100, Quantity =2, UnitPrice =100, DiscountPercent =0, IVAPercent =21 }
 }
 };

 unitOfWorkMock.Setup(u => u.SaleRepository.GetIncludingAsync(It.IsAny<int>(), It.IsAny<System.Linq.Expressions.Expression<System.Func<Sale, object>>[]>()))
 .ReturnsAsync(sale);

 deliveryNoteServiceMock.Setup(d => d.GetAllBySaleIdAsync(It.IsAny<int>()))
 .ReturnsAsync(new List<DeliveryNoteDTO>
 {
 new DeliveryNoteDTO
 {
 Id =1,
 Articles = new List<DeliveryNoteArticleDTO>
 {
 new DeliveryNoteArticleDTO { ArticleId =100, Quantity =2 }
 }
 }
 });

 // Provide a valid customer so service doesn't throw
 catalogClientMock.Setup(c => c.GetCustomerByIdAsync(10))
 .ReturnsAsync(new CustomerDTO { Id =10, CompanyName = "ACME SRL", TaxId = "20123456789", IVAType = "Responsable Inscripto", IVATypeArcaCode =1, PostalCodeId =1 });

 catalogClientMock.Setup(c => c.GetArticleByIdAsync(100))
 .ReturnsAsync(new SalesService.Business.Models.Clients.ArticleDTO { SKU = "PROD100", Description = "Test", Brand = "BrandX", Warranty =12 });

 fiscalClientMock.Setup(f => f.CreateFiscalNoteAsync(It.IsAny<FiscalDocumentCreateDTO>()))
 .ReturnsAsync(new FiscalDocumentResponseDTO { Id =123, TotalAmount =242 });

 var service = new SaleService(
 unitOfWorkMock.Object,
 catalogClientMock.Object,
 stockClientMock.Object,
 userClientMock.Object,
 fiscalClientMock.Object,
 accountingClientMock.Object,
 deliveryNoteServiceMock.Object,
 companyClientMock.Object
 );

 // Act
 var result = await service.CreateInvoiceAsync(1);

 // Assert
 fiscalClientMock.Verify(f => f.CreateFiscalNoteAsync(It.Is<FiscalDocumentCreateDTO>(dto => dto.TotalAmount ==242m)), Times.Once);
 Assert.Equal(123, result.Id);
 }
 }
}
