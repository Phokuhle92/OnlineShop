using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineShop.API.Interfaces;
using OnlineShop.API.Models.DTOs.OrderDTOs;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Customer")]  // Only Customers allowed
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrdersController(IOrderService orderService, UserManager<ApplicationUser> userManager)
    {
        _orderService = orderService;
        _userManager = userManager;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto orderDto)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User is not authorized to create orders." });

        try
        {
            var result = await _orderService.CreateOrderAsync(userId, orderDto);
            return Ok(new { message = "Order created successfully.", order = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Failed to create order: {ex.Message}" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User is not authorized to view orders." });

        var result = await _orderService.GetUserOrdersAsync(userId);
        if (result == null || result.Count == 0)
            return Ok(new { message = "No orders found for this user.", orders = result });

        return Ok(new { message = "Orders retrieved successfully.", orders = result });
    }

    [HttpPut("{orderId}")]
    public async Task<IActionResult> UpdateOrder(int orderId, [FromBody] CreateOrderDto orderDto)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User is not authorized to update orders." });

        try
        {
            var updatedOrder = await _orderService.UpdateOrderAsync(userId, orderId, orderDto);
            return Ok(new { message = "Order updated successfully.", order = updatedOrder });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Failed to update order: {ex.Message}" });
        }
    }

    [HttpDelete("{orderId}")]
    public async Task<IActionResult> DeleteOrder(int orderId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "User is not authorized to delete orders." });

        try
        {
            await _orderService.DeleteOrderAsync(userId, orderId);
            return Ok(new { message = "Order deleted successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = $"Failed to delete order: {ex.Message}" });
        }
    }
}
