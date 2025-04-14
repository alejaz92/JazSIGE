using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PurchaseService.Business.Interfaces;
using PurchaseService.Business.Models;
using PurchaseService.Controllers;
using Xunit;

namespace PurchaseService.Tests.Controllers
{
    public class DispatchControllerTests
    {
        private readonly Mock<IDispatchService> _dispatchServiceMock = new();
        private readonly DispatchController _controller;

        public DispatchControllerTests()
        {
            _controller = new DispatchController(_dispatchServiceMock.Object);
        }

        [Fact]
        public async Task GetAll_Should_Return_List()
        {
            var list = new List<DispatchDTO> { new DispatchDTO { Id = 1 } };
            _dispatchServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(list);

            var result = await _controller.GetAll();

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(list);
        }

        [Fact]
        public async Task GetAll_Should_Return_Empty_When_None()
        {
            _dispatchServiceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<DispatchDTO>());

            var result = await _controller.GetAll();
            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;

            var list = ok.Value.Should().BeAssignableTo<IEnumerable<DispatchDTO>>().Subject;
            list.Should().BeEmpty();
        }

        [Fact]
        public async Task GetPaged_Should_Return_Items_And_Total()
        {
            var list = new List<DispatchDTO> { new DispatchDTO { Id = 1 } };
            _dispatchServiceMock.Setup(s => s.GetAllAsync(1, 10)).ReturnsAsync((list, 5));

            var result = await _controller.GetPaged(1, 10);

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(new { totalCount = 5, items = list });
        }

        [Fact]
        public async Task GetPaged_Should_Return_Empty_When_None()
        {
            _dispatchServiceMock.Setup(s => s.GetAllAsync(1, 10)).ReturnsAsync((new List<DispatchDTO>(), 0));

            var result = await _controller.GetPaged(1, 10);

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().BeEquivalentTo(new { totalCount = 0, items = new List<DispatchDTO>() });
        }

        [Fact]
        public async Task GetById_Should_Return_Dispatch_When_Found()
        {
            var dto = new DispatchDTO { Id = 99 };
            _dispatchServiceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync(dto);

            var result = await _controller.GetById(99);

            var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            ok.Value.Should().Be(dto);
        }

        [Fact]
        public async Task GetById_Should_Return_404_When_Not_Found()
        {
            _dispatchServiceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync((DispatchDTO?)null);

            var result = await _controller.GetById(5);

            result.Result.Should().BeOfType<NotFoundResult>();
        }
    }
}
