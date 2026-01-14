using Moq;
using SalesService.Business.Services;
using SalesService.Business.Interfaces;
using SalesService.Business.Interfaces.Clients;
using SalesService.Business.Models.Clients;
using SalesService.Business.Models.Sale.fiscalDocs;
using SalesService.Business.Models.Sale;
using SalesService.Business.Models.DeliveryNote;
using SalesService.Infrastructure.Interfaces;
using SalesService.Infrastructure.Models.Sale;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Xunit;

namespace SalesService.Tests
{
 public class CreditNoteTests
 {
 [Fact]
 public async Task CreateCreditNote_PartialReturn_CallsFiscalAndAccountingAndStock()
 {
 // Arrange
 var unitOfWorkMock = new Mock<IUnitOfWork>();
 var saleRepoMock = new Mock<ISaleRepository>();
 var catalogClientMock = new Mock<ICatalogServiceClient>();
 var stockClientMock = new Mock<IStockServiceClient>();
 var userClientMock = new Mock<IUserServiceClient>();
 var fiscalClientMock = new Mock<IFiscalServiceClient>();
 var accountingClientMock = new Mock<IAccountingServiceClient>();
 var deliveryNoteServiceMock = new Mock<IDeliveryNoteService>();
 var companyClientMock = new Mock<ICompanyServiceClient>();

 var sale = new Sale
 {
 Id =2,
 IsFinalConsumer = false,
 CustomerId =20,
 CustomerIdType = "CUIT",
 CustomerTaxId = "20123456780",
 CustomerName = "ACME SRL",
 SellerId =3,
 ExchangeRate =1,
 HasInvoice = true,
 IsFullyDelivered = true,
 Articles = new List<Sale_Article>
 {
 new Sale_Article { ArticleId =200, Quantity =5, UnitPrice =50, DiscountPercent =0, IVAPercent =21 }
 }
 };

 saleRepoMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.IsAny<Expression<System.Func<Sale, object>>[]>() ))
 .ReturnsAsync(sale);

 unitOfWorkMock.Setup(u => u.SaleRepository).Returns(saleRepoMock.Object);

 // base invoice (existing)
 fiscalClientMock.Setup(f => f.GetBySaleIdAsync(It.IsAny<int>()))
 .ReturnsAsync(new FiscalDocumentResponseDTO { Id =10, TotalAmount =302.5m });

 // previous credit notes empty
 fiscalClientMock.Setup(f => f.GetCreditNotesAsync(It.IsAny<int>()))
 .ReturnsAsync(new List<FiscalDocumentResponseDTO>());

 // catalog article
 catalogClientMock.Setup(c => c.GetArticleByIdAsync(200))
 .ReturnsAsync(new ArticleDTO { SKU = "PROD200", Description = "X", Brand = "B", Warranty =6 });

 // catalog customer
 catalogClientMock.Setup(c => c.GetCustomerByIdAsync(20))
 .ReturnsAsync(new CustomerDTO { Id =20, CompanyName = "ACME SRL", TaxId = "20123456780", IVAType = "Responsable Inscripto", IVATypeArcaCode =1, PostalCodeId =1 });

 fiscalClientMock.Setup(f => f.CreateFiscalNoteAsync(It.IsAny<FiscalDocumentCreateDTO>()))
 .ReturnsAsync(new FiscalDocumentResponseDTO { Id =201, TotalAmount =121.0m });

 var dto = new CreditNoteCreateForSaleDTO
 {
 Reason = CreditNoteReason.PartialReturn,
 Items = new List<CreditNoteItemDTO>
 {
 new CreditNoteItemDTO { ArticleId =200, Quantity =1 }
 },
 ReturnWarehouseId =5
 };

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
 var result = await service.CreateCreditNoteAsync(2, dto);

 // Assert
 fiscalClientMock.Verify(f => f.CreateFiscalNoteAsync(It.IsAny<FiscalDocumentCreateDTO>()), Times.Once);
 accountingClientMock.Verify(a => a.UpsertExternalAsync(It.IsAny<AccountingExternalUpsertDTO>()), Times.Once);
 stockClientMock.Verify(s => s.RegisterQuickStockMovementAsync(It.IsAny<StockMovementCreateDTO>()), Times.Once);
 Assert.Equal(201, result.Id);
 }
 }
}
