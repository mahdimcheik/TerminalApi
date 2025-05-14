using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;

namespace TerminalApi.Services
{
    /// <summary>
    /// Service responsable de la gestion des adresses des utilisateurs.
    /// Permet de récupérer, ajouter, mettre à jour et supprimer des adresses.
    /// </summary>
    public class AddressService
    {
        private readonly UserManager<UserApp> userManager;
        private readonly ApiDefaultContext context;

        /// <summary>
        /// Initialise une nouvelle instance du service AddressService.
        /// </summary>
        /// <param name="userManager">Service UserManager pour gérer les utilisateurs.</param>
        /// <param name="context">Contexte de base de données pour accéder aux entités.</param>
        public AddressService(UserManager<UserApp> userManager, ApiDefaultContext context)
        {
            this.userManager = userManager;
            this.context = context;
        }

        /// <summary>
        /// Récupère la liste des adresses associées à un utilisateur.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <returns>Liste des adresses sous forme de DTO ou null si aucune adresse n'est trouvée.</returns>
        /// <remarks>
        /// Codes HTTP potentiels :
        /// - 200 : Succès.
        /// - 404 : Aucune adresse trouvée.
        /// - 500 : Erreur interne du serveur.
        /// </remarks>
        public async Task<List<AddressResponseDTO>?> GetAddresses(string userId)
        {
            return await context
                .Addresses.Where(ad => ad.UserId == userId)
                .Select(ad => ad.ToAddressDTO())
                .ToListAsync();
        }

        /// <summary>
        /// Ajoute une nouvelle adresse pour un utilisateur.
        /// </summary>
        /// <param name="addressCreate">Données de création de l'adresse.</param>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <returns>L'adresse ajoutée sous forme de DTO.</returns>
        /// <exception cref="Exception">Lève une exception si le nombre maximal d'adresses est atteint.</exception>
        /// <remarks>
        /// Codes HTTP potentiels :
        /// - 200 : Succès.
        /// - 400 : Nombre maximal d'adresses atteint.
        /// - 500 : Erreur interne du serveur.
        /// </remarks>
        public async Task<AddressResponseDTO> AddAddress(AddressCreateDTO addressCreate, string userId)
        {
            try
            {
                var listAdresses = await context.Addresses.Where(x => x.UserId == userId).ToListAsync();
                if (listAdresses.Count >= 5)
                {
                    throw new Exception("Nombre maximal d'adresses atteint");
                }
                var adress = addressCreate.ToAddress(userId);
                context.Addresses.Add(adress);
                await context.SaveChangesAsync();
                return adress.ToAddressDTO();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Met à jour une adresse existante.
        /// </summary>
        /// <param name="updatedAddressData">Données mises à jour de l'adresse.</param>
        /// <param name="address">Adresse existante à mettre à jour.</param>
        /// <returns>L'adresse mise à jour sous forme de DTO.</returns>
        /// <remarks>
        /// Codes HTTP potentiels :
        /// - 200 : Succès.
        /// - 404 : Adresse non trouvée.
        /// - 500 : Erreur interne du serveur.
        /// </remarks>
        public async Task<AddressResponseDTO> UpdateAddress(AddressUpdateDTO updatedAddressData, Address address)
        {
            try
            {
                updatedAddressData.ToAddress(address);
                await context.SaveChangesAsync();
                return address.ToAddressDTO();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Supprime une adresse associée à un utilisateur.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <param name="addressId">Identifiant de l'adresse à supprimer.</param>
        /// <returns>True si la suppression est réussie, sinon false.</returns>
        /// <remarks>
        /// Codes HTTP potentiels :
        /// - 200 : Succès.
        /// - 404 : Adresse non trouvée.
        /// - 500 : Erreur interne du serveur.
        /// </remarks>
        public async Task<bool> DeleteAddress(string userId, string addressId)
        {
            try
            {
                var address = await context.Addresses.FirstOrDefaultAsync(x => x.Id == Guid.Parse(addressId) && x.UserId == userId);
                if (address is null) throw new Exception("L'adresse n'existe pas");
                context.Addresses.Remove(address);
                await context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
