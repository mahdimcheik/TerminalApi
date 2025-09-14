using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{

    /// <summary>
    /// Contrôleur pour gérer les adresses des utilisateurs.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly UserManager<UserApp> userManager;
        private readonly ApiDefaultContext context;
        private readonly IAddressService addressService;

        /// <summary>
        /// Constructeur du contrôleur AddressController.
        /// </summary>
        /// <param name="userManager">Gestionnaire des utilisateurs.</param>
        /// <param name="context">Contexte de la base de données.</param>
        /// <param name="addressService">Service pour gérer les adresses.</param>
        public AddressController(
            UserManager<UserApp> userManager,
            ApiDefaultContext context,
            IAddressService addressService
        )
        {
            this.userManager = userManager;
            this.context = context;
            this.addressService = addressService;
        }

        /// <summary>
        /// Récupère toutes les adresses associées à un utilisateur donné.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <returns>Liste des adresses de l'utilisateur.</returns>
        [HttpGet("all")]
        public async Task<ActionResult<ResponseDTO<List<AddressResponseDTO>>>> GetAddressesByUserId([FromQuery] string userId)
        {
            if (userId.IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO<List<AddressResponseDTO>> { Status = 40, Message = "L'utilisateur n'existe pas" }
                );
            }
            try
            {
                var result = await addressService.GetAddresses(userId);
                return Ok(
                    new ResponseDTO<List<AddressResponseDTO>> {
                        Status = 200,
                        Message = "Liste d'adresses envoyée",
                        Data = result,
                    }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<List<AddressResponseDTO>> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Ajoute une nouvelle adresse pour l'utilisateur connecté.
        /// </summary>
        /// <param name="addressCreate">Données de l'adresse à ajouter.</param>
        /// <returns>Adresse ajoutée.</returns>
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<AddressResponseDTO>>> AddAddress([FromBody] AddressCreateDTO addressCreate)
        {
            if (addressCreate is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 400, Message = "Demande refusée" });
            }
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refusée" });
            }

            try
            {
                var result = await addressService.AddAddress(addressCreate, user.Id);
                return Ok(new ResponseDTO<object> { Data = result, Message = "Adresse ajoutée", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Met à jour une adresse existante pour l'utilisateur connecté.
        /// </summary>
        /// <param name="addressDTO">Données de l'adresse à mettre à jour.</param>
        /// <returns>Adresse mise à jour.</returns>
        [HttpPut]
        public async Task<ActionResult<ResponseDTO<AddressResponseDTO>>> UpdateAddress([FromBody] AddressUpdateDTO addressDTO)
        {
            if (addressDTO is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refusée" });
            }
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);

            if (user is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refusée" });
            }

            var addressFromDb = await context.Addresses.FirstOrDefaultAsync(x => x.Id == Guid.Parse(addressDTO.Id) && x.UserId == user.Id);

            if (addressFromDb is null)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refusée" });
            }

            try
            {
                var result = await addressService.UpdateAddress(addressDTO, addressFromDb);
                return Ok(new ResponseDTO<object> { Data = result, Message = "Adresse mise à jour", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime une adresse existante pour l'utilisateur connecté.
        /// </summary>
        /// <param name="addressId">Identifiant de l'adresse à supprimer.</param>
        /// <returns>Confirmation de suppression.</returns>
        [HttpDelete]
        public async Task<ActionResult<ResponseDTO<object>>> DeleteAddress([FromQuery] string addressId)
        {
            try
            {
                if (addressId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refusée" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<object> { Status = 40, Message = "Demande refusée" });
                }
                var resultDelete = await addressService.DeleteAddress(user.Id, addressId);
                return Ok(new ResponseDTO<object> { Message = "L'adresse est supprimée", Status = 204 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<object> { Status = 40, Message = ex.Message });
            }
        }
    }
}
