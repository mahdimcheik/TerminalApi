using TerminalApi.Utilities;

namespace TerminalApi.Models
{
    public static class OrderExtension
    {
        public static OrderResponseForStudentDTO ToOrderResponseForStudentDTO(this Order order)
        {
            var response = new OrderResponseForStudentDTO
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                PaymentDate = order.PaymentDate,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                CheckoutExpiredAt = order.CheckoutExpiredAt,
                CheckoutID = order.CheckoutID,
                UpdatedAt = order.UpdatedAt,
            };
            if (order.Bookings is not null && order.Bookings.Any())
            {
                response.Bookings = order
                    .Bookings.Select(x => x.ToBookingResponseDTO(order.Booker))
                    .ToList();
                response.TotalDiscountedPrice = order.TotalDiscountedPrice;
                response.TotalOriginalPrice = order.TotalOriginalPrice;
                response.TotalReduction = order.TotalReduction;
                response.PaymentIntent = order.PaymentIntent;
            }

            var timeToPay = EnvironmentVariables.HANGFIRE_ORDER_CLEANING_DELAY;
            if (
                order.UpdatedAt is null
                || (order.UpdatedAt + TimeSpan.FromMinutes(timeToPay)) < DateTimeOffset.UtcNow
            )
            {
                response.LeftTimeToPay = new TimespanDTO(0, 0);
            }
            else
            {
                var res =
                    (order.UpdatedAt + TimeSpan.FromMinutes(timeToPay) - DateTimeOffset.UtcNow)
                    ?? TimeSpan.Zero;
                response.LeftTimeToPay = new TimespanDTO(res.Minutes, res.Seconds);
            }

            return response;
        }

        public static OrderResponseForTeacherDTO ToOrderResponseForTeacherDTO(this Order order)
        {
            var response = new OrderResponseForTeacherDTO
            {
                OrderNumber = order.OrderNumber,
                PaymentDate = order.PaymentDate,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,
                UpdatedAt = order.UpdatedAt,
            };
            if (order.Bookings is not null && order.Bookings.Any())
            {
                response.Bookings = order.Bookings.Select(x => x.ToBookingResponseDTO()).ToList();
                response.TotalDiscountedPrice = order.TotalDiscountedPrice;
                response.TotalOriginalPrice = order.TotalOriginalPrice;
                response.TotalReduction = order.TotalReduction;
                response.PaymentIntent = order.PaymentIntent;
            }
            return response;
        }

        public static void Reset(this Order order)
        {
            order.Bookings.Clear();
            order.UpdatedAt = DateTimeOffset.Now;
            order.CheckoutExpiredAt = null;
            order.CheckoutID = null;
            order.PaymentIntent = null;
            order.PaymentMethod = "";
            order.Status = EnumBookingStatus.Pending;
        }

        public static void ResetCheckout(this Order order)
        {
            order.UpdatedAt = DateTimeOffset.Now;
            order.CheckoutExpiredAt = null;
            order.CheckoutID = null;
            order.PaymentIntent = null;
            order.PaymentMethod = "";
            order.Status = EnumBookingStatus.Pending;
        }
    }
}
