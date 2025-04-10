using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using System;
using TerminalApi.Models.Bookings;
using TerminalApi.Models.TVA;
using TerminalApi.Models.User;

namespace TerminalApi.Models.Payments
{
    public static class OrderExtension
    {

        public static OrderResponseForStudentDTO ToOrderResponseForStudentDTO(this Order order)
        {
            var response =  new OrderResponseForStudentDTO
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                PaymentDate = order.PaymentDate,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod,


            };
            if(order.Bookings is not null && order.Bookings.Any())
            {
                response.Bookings = order.Bookings.Select(x => x.ToBookingResponseDTO(order.Booker)).ToList();
                response.TotalDiscountedPrice = order.TotalDiscountedPrice;
                response.TotalOriginalPrice = order.TotalOriginalPrice;
                response.TotalReduction = order.TotalReduction;
            }
            return response;
        }
        public static OrderResponseForTeacherDTO ToOrderResponseForTeacherDTO(this Order order)
        {
            var response =  new OrderResponseForTeacherDTO
            {
                OrderNumber = order.OrderNumber,
                PaymentDate = order.PaymentDate,
                CreatedAt = order.CreatedAt,
                Status = order.Status,
                PaymentMethod = order.PaymentMethod
            };
            if (order.Bookings is not null && order.Bookings.Any())
            {
                response.Bookings = order.Bookings.Select(x => x.ToBookingResponseDTO()).ToList();
                response.TotalDiscountedPrice = order.TotalDiscountedPrice;
                response.TotalOriginalPrice = order.TotalOriginalPrice;
                response.TotalReduction = order.TotalReduction;
            }
            return response;
        }
    }

}
