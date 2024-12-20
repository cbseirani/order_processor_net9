using Microsoft.Extensions.Logging;
using Moq;
using OrderProcessorService.Clients;
using OrderProcessorService.Models;
using OrderProcessorService.Services;
using Xunit;

namespace OrderProcessorService.Tests;

public class OrderProcessorTests
{
    private readonly Mock<IApiClient> _apiClient = new();
    private readonly Mock<ILogger<OrderService>> _logger = new();
    
    [Fact]
    public async Task ProcessOrdersAsync_UpdatesDeliveredOrders()
    {
        // Arrange
        _apiClient.Setup(x => x.GetOrders()).ReturnsAsync(new List<Order>
        {
            new () { OrderId = "1", Status = "Delivered" },
            new () { OrderId = "2", Status = "Pending" }
        });

        // Act
        await CreateProcessor().ProcessOrders();

        // Assert
        _apiClient.Verify(x => x.SendDeliveryNotification(It.Is<string>(id => id.Equals("1"))), Times.Once);
        _apiClient.Verify(x => x.UpdateOrderStatus(It.Is<Order>(order => order.OrderId.Equals("1"))), Times.Once);
        _apiClient.Verify(x => x.SendDeliveryNotification(It.Is<string>(id => id.Equals("2"))), Times.Never);
    }

    private OrderService CreateProcessor() => new (_apiClient.Object, _logger.Object);
}