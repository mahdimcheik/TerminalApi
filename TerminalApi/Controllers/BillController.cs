﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Payments;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des factures liées aux commandes.
    /// Permet de générer et de récupérer des factures au format PDF.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class BillController : ControllerBase
    {
        private readonly PdfService pdfService;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur <see cref="BillController"/>.
        /// </summary>
        /// <param name="pdfService">Service utilisé pour générer des fichiers PDF.</param>
        /// <param name="context">Contexte de base de données pour accéder aux commandes et autres entités.</param>
        public BillController(PdfService pdfService, ApiDefaultContext context)
        {
            this.pdfService = pdfService;
            this.context = context;
        }

        /// <summary>
        /// Récupère une facture au format PDF pour une commande donnée.
        /// </summary>
        /// <param name="orderId">Identifiant unique de la commande pour laquelle générer la facture.</param>
        /// <returns>
        /// Un fichier PDF contenant la facture si la commande est trouvée.
        /// Retourne un code HTTP 200 avec le fichier PDF, 
        /// un code HTTP 404 si la commande n'existe pas, 
        /// ou un code HTTP 400 si l'identifiant de la commande est invalide.
        /// </returns>
        /// <response code="200">Facture générée avec succès.</response>
        /// <response code="400">Requête invalide, identifiant de commande manquant ou incorrect.</response>
        /// <response code="404">Commande non trouvée.</response>
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string orderId)
        {
            Console.WriteLine(orderId);

            // Vérifie si l'identifiant de commande est vide ou invalide
            if (orderId.Trim().IsNullOrEmpty())
            {
                return BadRequest(
                    new ResponseDTO { Message = "Aucune commande correspondante", Status = 404, }
                );
            }

            // Recherche de la commande dans la base de données
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
                    new ResponseDTO { Message = "Aucune commande correspondante", Status = 404, }
                );
            }

            // Génération du fichier PDF via le service PdfService
            var file = await pdfService.GeneratePdfAsync(order.ToOrderResponseForStudentDTO());

            // Retourne le fichier PDF avec un code 200
            return File(file, "application/pdf", "facture.pdf");
        }
    }
}
