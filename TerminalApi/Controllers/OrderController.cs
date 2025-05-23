﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Payments;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService orderService;
        private readonly ApiDefaultContext context;

        public OrderController(OrderService orderService, ApiDefaultContext context)
        {
            this.orderService = orderService;
            this.context = context;
        }
        [HttpPost("student/all")]
        public async Task<ActionResult<ResponseDTO>>  GetOrderForStudentPaginated(
            [FromBody] OrderPagination query
        )
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            var response = await orderService.GetOrdersForStudentPaginatedAsync(query, user);

            return Ok(
               response
            );
        }

        [HttpPost("teacher/all")]
        public async Task<ActionResult<ResponseDTO>> GetOrderForTeacherPaginated(
    [FromBody] OrderPagination query
)
        {
            var orders = await orderService.GetOrdersForTeacherPaginatedAsync(query);
            if (orders is null || orders.Count == 0)
                return NotFound(
                    new ResponseDTO { Message = "Aucune commande disponible", Status = 404 }
                );
            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = orders
                }
            );
        }

        [HttpGet("student/{orderId}")]
        public async Task<ActionResult<ResponseDTO>> GetOrderByStudent(Guid orderId)
        {
            var order = await orderService.GetOrderByStudentAsync(orderId);
            if (order is null)
                return NotFound(
                    new ResponseDTO { Message = "Aucune commande disponible", Status = 404 }
                );
            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = order
                }
            );
        }

        

        [HttpGet("teacher/{orderId}")]
        public async Task<ActionResult<ResponseDTO>> GetOrderByTeacher(Guid orderId)
        {
            var order = await orderService.GetOrderByTeacherAsync(orderId);
            if (order is null)
                return NotFound(
                    new ResponseDTO { Message = "Aucune commande disponible", Status = 404 }
                );
            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = order
                }
            );
        }

        [HttpGet("student/current")]
        public async Task<ActionResult<ResponseDTO>> GetCurrentOrder()
        {
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            var order = await orderService.GetOrCreateCurrentOrderByUserAsync(user);
            if (order is null)
            {
                return NotFound(
                    new ResponseDTO { Message = "Aucune commande disponible", Status = 404 }
                );
            }
            return Ok(
                new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Status = 200,
                    Data = order
                }
            );
        }
    }
}
