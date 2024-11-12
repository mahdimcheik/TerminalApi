using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Formats.Asn1;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Adresse;
using TerminalApi.Models.User;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly UserManager<UserApp> userManager;
        private readonly ApiDefaultContext context;
        private readonly AddressService addressService;

        public AddressController(
            UserManager<UserApp> userManager,
            ApiDefaultContext context,
            AddressService addressService
        )
        {
            this.userManager = userManager;
            this.context = context;
            this.addressService = addressService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<ResponseDTO>> GetAddressesByUserId([FromQuery] string userId)
        {
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO { Status = 404, Message = "L'utilisateur n'existe pas" }
                );
            }
            try
            {
                var result = await addressService.GetAddresses(userId);
                return Ok(
                    new ResponseDTO
                    {
                        Status = 200,
                        Message = "Liste d'addresses envoyée",
                        Data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ResponseDTO>> AddAddress([FromBody] AddressCreateDTO addressCreate)
        
        {
            if(addressCreate is null)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }
            var res = await CheckUser.CheckUserNullByUserId(addressCreate.UserId, userManager);
            if (res)
            {
                return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
            }

            try
            {
                var result = await addressService.AddAddress(addressCreate);
                return Ok(new ResponseDTO { Data = result, Message = "Addresse ajoutée", Status = 200 });
            }
            catch (Exception ex) { 
                return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
            }

        }

        //[HttpPatch]
        //public async Task<ActionResult<ResponseDTO>> UpdateAddress([FromBody] Address address)

        //{
        //    if (address is null)
        //    {
        //        return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
        //    }
        //    var res = await CheckUser.CheckUserNullByUserId(addressCreate.UserId, userManager);
        //    if (res)
        //    {
        //        return BadRequest(new ResponseDTO { Status = 400, Message = "Demande refusée" });
        //    }

        //    try
        //    {
        //        var result = await addressService.AddAddress(addressCreate);
        //        return Ok(new ResponseDTO { Data = result, Message = "Addresse ajoutée", Status = 200 });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new ResponseDTO { Status = 400, Message = ex.Message });
        //    }

        //}
    }

   
}
