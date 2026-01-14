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
 public class DebitNoteTests
 {
 [Fact]
 public async Task CreateDebitNote_CallsFiscalAndAccounting()
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
 Id =3,
 IsFinalConsumer = false,
 CustomerId =30,
 CustomerIdType = "CUIT",
 CustomerTaxId = "20123456781",
 CustomerName = "ACME SRL",
 SellerId =4,
 ExchangeRate =1,
 HasInvoice = true,
 IsFullyDelivered = true,
 Articles = new List<Sale_Article>
 {
 new Sale_Article { ArticleId =300, Quantity =1, UnitPrice =500, DiscountPercent =0, IVAPercent =21 }
 }
 };

 saleRepoMock.Setup(r => r.GetIncludingAsync(It.IsAny<int>(), It.Is<Expression<System.Func<Sale, object>>[]>(arr => true) ))
 .ReturnsAsync(sale);

 unitOfWorkMock.Setup(u => u.SaleRepository).Returns(saleRepoMock.Object);

 fiscalClientMock.Setup(f => f.GetBySaleIdAsync(It.IsAny<int>()))
 .ReturnsAsync(new FiscalDocumentResponseDTO { Id =11, TotalAmount =605m });

 catalogClientMock.Setup(c => c.GetArticleByIdAsync(300))
 .ReturnsAsync(new ArticleDTO { SKU = "PROD300", Description = "Y", Brand = "B2", Warranty =12 });

 fiscalClientMock.Setup(f => f.CreateFiscalNoteAsync(It.IsAny<FiscalDocumentCreateDTO>()))
 .ReturnsAsync(new FiscalDocumentResponseDTO { Id =301, TotalAmount =110 });

 var dto = new DebitNoteCreateForSaleDTO
 {
 NetAmount =100,
 VatPercent =21
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
 var result = await service.CreateDebitNoteAsync(3, dto);

 // Assert
 fiscalClientMock.Verify(f => f.CreateFiscalNoteAsync(It.IsAny<FiscalDocumentCreateDTO>()), Times.Once);
 accountingClientMock.Verify(a => a.UpsertExternalAsync(It.IsAny<AccountingExternalUpsertDTO>()), Times.Once);
 Assert.Equal(301, result.Id);
 }
 }
}
