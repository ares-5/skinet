using API.Extensions;
using API.SignalR;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stripe;

namespace API.Controllers;

public class PaymentController(
    IPaymentService paymentService,
    IUnitOfWork unit,
    IConfiguration config,
    IHubContext<NotificationHub> hubContext,
    ILogger<PaymentController> logger) : BaseApiController
{
    private readonly IPaymentService paymentService = paymentService;
    private readonly IUnitOfWork unit = unit;
    private readonly IHubContext<NotificationHub> hubContext = hubContext;
    private readonly ILogger<PaymentController> logger = logger;
    private readonly string webhookSecret = config["StripeSettings:WhSecret"]!;

    [Authorize]
    [HttpPost("{cartId}")]
    public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cartId)
    {
        var cart = await paymentService.CreateOrUpdatePaymentIntent(cartId);
        if (cart is null)
        {
            return BadRequest("Problem with your cart");
        }

        return Ok(cart);
    }

    [HttpGet("delivery-methods")]
    public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
    {
        return Ok(await unit.Repository<DeliveryMethod>().ListAllAsync());
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhookAsync()
    {
        var json = await new StreamReader(Request.Body).ReadToEndAsync();
        try
        {
            var stripeEvent = ConstructStripeEvent(json);
            if (stripeEvent.Data.Object is not PaymentIntent intent)
            {
                return BadRequest("Invalid event data");
            }

            await HandlePaymentIntentSuccess(intent);
            return Ok();
        }
        catch (StripeException ex)
        {
            logger.LogError(ex, "Stripe webhook exception");
            return StatusCode(StatusCodes.Status500InternalServerError, "Webhook error");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred");
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
        }
    }

    private async Task HandlePaymentIntentSuccess(PaymentIntent intent)
    {
        if (intent.Status == "succeeded")
        {
            var spec = new OrderSpecification(intent.Id, isPaymentIntent: true);
            var order = await unit.Repository<Order>().GetEntityWithSpec(spec)
                        ?? throw new Exception("Order not found");

            if ((long)order.GetTotal() * 100 != intent.Amount)
            {
                order.Status = OrderStatus.PaymentMismatch;
            }
            else
            {
                order.Status = OrderStatus.PaymentReceived;
            }
            
            await unit.CompleteAsync();

            var connectionId = NotificationHub.GetConnectionIdByEmail(User.GetEmail());
            if (!string.IsNullOrWhiteSpace(connectionId))
            {
                await hubContext.Clients.Client(connectionId).SendAsync("OrderCompleteNotification", order.ToDto());
            }
        }
    }
    
    private Event ConstructStripeEvent(string json)
    {
        try
        {
            return EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to construct Stripe event");
            throw new Exception("Invalid signature");
        }
    }
}