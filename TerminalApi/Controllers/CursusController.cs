using Microsoft.AspNetCore.Mvc;
using TerminalApi.Models;
using TerminalApi.Services;

namespace TerminalApi.Controllers
{
    /// <summary>
    /// Contrôleur responsable de la gestion des cursus.
    /// Permet de créer, lire, mettre à jour et supprimer des cursus.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class CursusController : ControllerBase
    {
        private readonly ICursusService _cursusService;

        /// <summary>
        /// Initialise une nouvelle instance du contrôleur CursusController.
        /// </summary>
        /// <param name="cursusService">Service pour la gestion des cursus.</param>
        public CursusController(ICursusService cursusService)
        {
            _cursusService = cursusService;
        }

        /// <summary>
        /// Récupère tous les cursus avec filtrage optionnel.
        /// </summary>
        /// <param name="filter">Filtres optionnels pour la recherche.</param>
        /// <returns>Liste des cursus correspondant aux critères.</returns>
        /// <response code="200">Retourne la liste des cursus.</response>
        /// <response code="500">Erreur interne du serveur.</response>
        [HttpPost("all")]
        [ProducesResponseType(typeof(ResponseDTO<IEnumerable<CursusDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<IEnumerable<CursusDto>>>> GetAll([FromBody] QueryPagination query)
        {
            try
            {
                var cursus = await _cursusService.GetAllAsync(query);
                return Ok(new ResponseDTO<IEnumerable<CursusDto>>
                {
                    Status = 200,
                    Message = "Cursus récupérés avec succès",
                    Data = cursus.Item1,
                    Count = cursus.Item2
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<string>
                {
                    Status = 500,
                    Message = "Erreur lors de la récupération des cursus",
                    Data = ex.Message
                });
            }
        }

        /// <summary>
        /// Crée un nouveau cursus.
        /// </summary>
        /// <param name="dto">Données du cursus à créer.</param>
        /// <returns>Le cursus créé.</returns>
        /// <response code="201">Cursus créé avec succès.</response>
        /// <response code="400">Données invalides.</response>
        /// <response code="500">Erreur interne du serveur.</response>
        [HttpPost]
        [ProducesResponseType(typeof(ResponseDTO<CursusDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseDTO<string?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<CursusDto>>> Create([FromBody] CreateCursusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO<string?>
                {
                    Status = 400,
                    Message = "Données invalides"
                });
            }

            try
            {
                var cursus = await _cursusService.CreateAsync(dto);
                return Ok( new ResponseDTO<CursusDto>
                {
                    Status = 201,
                    Message = "Cursus créé avec succès",
                    Data = cursus
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDTO<string?>
                {
                    Status = 400,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<string>
                {
                    Status = 500,
                    Message = "Erreur lors de la création du cursus",
                    Data = ex.Message
                });
            }
        }

        /// <summary>
        /// Met à jour un cursus existant.
        /// </summary>
        /// <param name="id">ID du cursus à mettre à jour.</param>
        /// <param name="dto">Nouvelles données du cursus.</param>
        /// <returns>Le cursus mis à jour.</returns>
        /// <response code="200">Cursus mis à jour avec succès.</response>
        /// <response code="400">Données invalides.</response>
        /// <response code="404">Cursus non trouvé.</response>
        /// <response code="500">Erreur interne du serveur.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ResponseDTO<CursusDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string?>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ResponseDTO<string?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<CursusDto>>> Update(Guid id, [FromBody] UpdateCursusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseDTO<string?>
                {
                    Status = 400,
                    Message = "Données invalides"
                });
            }

            try
            {
                var cursus = await _cursusService.UpdateAsync(id, dto);
                if (cursus == null)
                {
                    return NotFound(new ResponseDTO<string?>
                    {
                        Status = 404,
                        Message = "Cursus non trouvé"
                    });
                }

                return Ok(new ResponseDTO<CursusDto>
                {
                    Status = 200,
                    Message = "Cursus mis à jour avec succès",
                    Data = cursus
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ResponseDTO<string?>
                {
                    Status = 400,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<string>
                {
                    Status = 500,
                    Message = "Erreur lors de la mise à jour du cursus",
                    Data = ex.Message
                });
            }
        }

        /// <summary>
        /// Supprime un cursus.
        /// </summary>
        /// <param name="id">ID du cursus à supprimer.</param>
        /// <returns>Confirmation de la suppression.</returns>
        /// <response code="200">Cursus supprimé avec succès.</response>
        /// <response code="404">Cursus non trouvé.</response>
        /// <response code="500">Erreur interne du serveur.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ResponseDTO<string?>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string?>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<string?>>> Delete(Guid id)
        {
            try
            {
                var success = await _cursusService.DeleteAsync(id);
                if (!success)
                {
                    return NotFound(new ResponseDTO<string?>
                    {
                        Status = 404,
                        Message = "Cursus non trouvé"
                    });
                }

                return Ok(new ResponseDTO<string?>
                {
                    Status = 200,
                    Message = "Cursus supprimé avec succès"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<string>
                {
                    Status = 500,
                    Message = "Erreur lors de la suppression du cursus",
                    Data = ex.Message
                });
            }
        }

        /// <summary>
        /// Récupère tous les niveaux disponibles.
        /// </summary>
        /// <returns>Liste des niveaux.</returns>
        /// <response code="200">Retourne la liste des niveaux.</response>
        /// <response code="500">Erreur interne du serveur.</response>
        [HttpGet("levels")]
        [ProducesResponseType(typeof(ResponseDTO<IEnumerable<Level>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<IEnumerable<Level>>>> GetLevels()
        {
            try
            {
                var levels = await _cursusService.GetLevelsAsync();
                return Ok(new ResponseDTO<IEnumerable<Level>>
                {
                    Status = 200,
                    Message = "Niveaux récupérés avec succès",
                    Data = levels,
                    Count = levels.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<string>
                {
                    Status = 500,
                    Message = "Erreur lors de la récupération des niveaux",
                    Data = ex.Message
                });
            }
        }

        /// <summary>
        /// Récupère toutes les catégories disponibles.
        /// </summary>
        /// <returns>Liste des catégories.</returns>
        /// <response code="200">Retourne la liste des catégories.</response>
        /// <response code="500">Erreur interne du serveur.</response>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(ResponseDTO<IEnumerable<Category>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseDTO<string>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ResponseDTO<IEnumerable<Category>>>> GetCategories()
        {
            try
            {
                var categories = await _cursusService.GetCategoriesAsync();
                return Ok(new ResponseDTO<IEnumerable<Category>>
                {
                    Status = 200,
                    Message = "Catégories récupérées avec succès",
                    Data = categories,
                    Count = categories.Count()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDTO<string>
                {
                    Status = 500,
                    Message = "Erreur lors de la récupération des catégories",
                    Data = ex.Message
                });
            }
        }
    }
} 