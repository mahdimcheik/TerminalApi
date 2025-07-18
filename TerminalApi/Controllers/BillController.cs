using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Interfaces;
using TerminalApi.Models;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contr�leur responsable de la gestion des factures li�es aux commandes.
    /// Permet de g�n�rer et de r�cup�rer des factures au format PDF.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    //[Authorize]
    public class BillController : ControllerBase
    {
        private readonly IPdfService pdfService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contr�leur <see cref="BillController"/>.
        /// </summary>
        /// <param name="pdfService">Service utilis� pour g�n�rer des fichiers PDF.</param>
        /// <param name="context">Contexte de base de donn�es pour acc�der aux commandes et autres entit�s.</param>
        public BillController(IPdfService pdfService, ApiDefaultContext context)
        {
            this.pdfService = pdfService;
            this.context = context;
        }

        /// <summary>
        /// R�cup�re une facture au format PDF pour une commande donn�e.
        /// </summary>
        /// <param name="orderId">Identifiant unique de la commande pour laquelle g�n�rer la facture.</param>
        /// <returns>
        /// Un fichier PDF contenant la facture si la commande est trouv�e.
        /// Retourne un code HTTP 200 avec le fichier PDF, 
        /// un code HTTP 404 si la commande n'existe pas, 
        /// ou un code HTTP 400 si l'identifiant de la commande est invalide.
        /// </returns>
        /// <response code="200">Facture g�n�r�e avec succ�s.</response>
        /// <response code="400">Requ�te invalide, identifiant de commande manquant ou incorrect.</response>
        /// <response code="404">Commande non trouv�e.</response>
        [HttpGet("export")]
        [Produces("application/pdf")]
        public async Task<IActionResult> Get([FromQuery] string orderId)
        {
            Console.WriteLine(orderId);

            // V�rifie si l'identifiant de commande est vide ou invalide
            if (orderId.Trim().IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO<object> { Message = "Aucune commande correspondante", Status = 404, }
                );
            }

            // Recherche de la commande dans la base de donn�es
            var order = context
                .Orders.Where(x => x.Id == Guid.Parse(orderId))
                .Include(x => x.Booker)
                .Include(x => x.Bookings)
                .ThenInclude(x => x.Slot)
                .FirstOrDefault();

            // Si la commande n'existe pas, retourne un code 404
            if (order is null)
            {
                return BadRequest(
                    new ResponseDTO<object> { Message = "Aucune commande correspondante", Status = 404, }
                );
            }

            // G�n�ration du fichier PDF via le service PdfService
            var file = await pdfService.GeneratePdfAsync(order.ToOrderResponseForStudentDTO());

            // Retourne le fichier PDF avec un code 200
            return File(file, "application/pdf", "facture.pdf");
        }
    }
}
