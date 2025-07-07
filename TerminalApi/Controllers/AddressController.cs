using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Services;
using TerminalApi.Utilities;

namespace TerminalApi.Controllers
{

    /// <summary>
    /// Contr�leur pour g�rer les adresses des utilisateurs.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly UserManager<UserApp> userManager;
        private readonly ApiDefaultContext context;
        private readonly AddressService addressService;

        /// <summary>
        /// Constructeur du contr�leur AddressController.
        /// </summary>
        /// <param name="userManager">Gestionnaire des utilisateurs.</param>
        /// <param name="context">Contexte de la base de donn�es.</param>
        /// <param name="addressService">Service pour g�rer les adresses.</param>
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

        /// <summary>
        /// R�cup�re toutes les adresses associ�es � un utilisateur donn�.
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
                        Message = "Liste d'adresses envoy�e",
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
        /// Ajoute une nouvelle adresse pour l'utilisateur connect�.
        /// </summary>
        /// <param name="addressCreate">Donn�es de l'adresse � ajouter.</param>
        /// <returns>Adresse ajout�e.</returns>
        [HttpPost]
        public async Task<ActionResult<ResponseDTO<AddressResponseDTO>>> AddAddress([FromBody] AddressCreateDTO addressCreate)
        {
            if (addressCreate is null)
            {
                return BadRequest(new ResponseDTO<AddressResponseDTO> { Status = 40, Message = "Demande refus�e" });
            }
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
            if (user is null)
            {
                return BadRequest(new ResponseDTO<AddressResponseDTO> { Status = 40, Message = "Demande refus�e" });
            }

            try
            {
                var result = await addressService.AddAddress(addressCreate, user.Id);
                return Ok(new ResponseDTO<AddressResponseDTO> { Data = result, Message = "Adresse ajout�e", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<AddressResponseDTO> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Met � jour une adresse existante pour l'utilisateur connect�.
        /// </summary>
        /// <param name="addressDTO">Donn�es de l'adresse � mettre � jour.</param>
        /// <returns>Adresse mise � jour.</returns>
        [HttpPut]
        public async Task<ActionResult<ResponseDTO<AddressResponseDTO>>> UpdateAddress([FromBody] AddressUpdateDTO addressDTO)
        {
            if (addressDTO is null)
            {
                return BadRequest(new ResponseDTO<AddressResponseDTO> { Status = 40, Message = "Demande refus�e" });
            }
            var user = CheckUser.GetUserFromClaim(HttpContext.User, context);

            if (user is null)
            {
                return BadRequest(new ResponseDTO<AddressResponseDTO> { Status = 40, Message = "Demande refus�e" });
            }

            var AddressFromDb = await context.Addresses.FirstOrDefaultAsync(x => x.Id == Guid.Parse(addressDTO.Id) && x.UserId == user.Id);

            if (AddressFromDb is null)
            {
                return BadRequest(new ResponseDTO<AddressResponseDTO> { Status = 40, Message = "Demande refus�e" });
            }

            try
            {
                var result = await addressService.UpdateAddress(addressDTO, AddressFromDb);
                return Ok(new ResponseDTO<AddressResponseDTO> { Data = result, Message = "Adresse mise � jour", Status = 200 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<AddressResponseDTO> { Status = 40, Message = ex.Message });
            }
        }

        /// <summary>
        /// Supprime une adresse pour l'utilisateur connect�.
        /// </summary>
        /// <param name="addressId">Identifiant de l'adresse � supprimer.</param>
        /// <returns>Statut de la suppression.</returns>
        [HttpDelete]
        public async Task<ActionResult<ResponseDTO<string?>>> DeleteAddress([FromQuery] string addressId)
        {
            try
            {
                if (addressId.IsNullOrEmpty())
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e" });
                }
                var user = CheckUser.GetUserFromClaim(HttpContext.User, context);
                if (user is null)
                {
                    return BadRequest(new ResponseDTO<string?> { Status = 40, Message = "Demande refus�e" });
                }
                var resultDelete = await addressService.DeleteAddress(user.Id, addressId);
                return Ok(new ResponseDTO<string?> { Message = "L'adresse est supprim�e", Status = 204 });
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseDTO<string?> { Status = 40, Message = ex.Message });
            }
        }
    }


}
