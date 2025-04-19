using System.ComponentModel;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TerminalApi.Contexts;
using TerminalApi.Models;
using TerminalApi.Models.Notification;
using TerminalApi.Models.User;
using TerminalApi.Utilities;

namespace TerminalApi.Services
{
    public class UsersService
    {
        private readonly ApiDefaultContext context;
        private readonly UserManager<UserApp> _userManager;
        private readonly NotificationService notificationService;

        public UsersService(
            ApiDefaultContext context,
            UserManager<UserApp> userManager,
            NotificationService notificationService
        )
        {
            this.context = context;
            _userManager = userManager;
            this.notificationService = notificationService;
        }

        public async Task<UserApp?> GetTeacherUser()
        {
            return await context.Users.FirstOrDefaultAsync(x =>
                x.Id == EnvironmentVariables.TEACHER_ID
            );
        }

        public async Task<ResponseDTO> GetAllStudentsDTO(QueryPagination query)
        {
            var querySql = context.Users.Where(x => x.EmailConfirmed && x.Id != HardCode.TeacherId);

            var count = await querySql.CountAsync();

            if (query is null)
            {
                return new ResponseDTO
                {
                    Message = "Demande acceptée",
                    Count = count,
                    Data = querySql.Skip(0).Take(10).ToList()
                };
            }

            querySql = querySql.Skip(query?.Start ?? 0).Take(query?.PerPage ?? 10);

            var result = await querySql.ToListAsync();
            return new ResponseDTO
            {
                Message = "Demande acceptée",
                Count = count,
                Data = result
            };
        }

        public async Task<ResponseDTO> Update(UserUpdateDTO model, ClaimsPrincipal UserPrincipal)
        {
            var user = CheckUser.GetUserFromClaim(UserPrincipal, context);
            if (user is null)
            {
                return new ResponseDTO
                {
                    Status = 404,
                    Message = "Le compte n'existe pas ou ne correspond pas",
                };
            }
            model.ToUser(user);
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.SaveChangesAsync();
                await notificationService.AddNotification(
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        RecipientId = user.Id,
                        SenderId = user.Id,
                        Type = EnumNotificationType.AccountUpdated
                    }
                );
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseDTO { Status = 401, Message = ex.Message };
            }

            return new ResponseDTO
            {
                Message = "Profil mis à jour",
                Status = 200,
                Data = user.ToUserResponseDTO(),
            };
        }

        public async Task<ResponseDTO> BanUnbanUser( UserBanDTO userBanDTO)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userBanDTO.UserId);
            if (user is null)
            {
                return new ResponseDTO
                {
                    Status = 404,
                    Message = "Le compte n'existe pas ou ne correspond pas",
                };
            }
            user.IsBanned = userBanDTO.IsBanned;
            user.BannedUntilDate = (userBanDTO.BannedUntilDate is null) ? DateTimeOffset.UtcNow.AddDays(1) : userBanDTO.BannedUntilDate;
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.SaveChangesAsync();
                await notificationService.AddNotification(
                    new Notification
                    {
                        Id = Guid.NewGuid(),
                        RecipientId = user.Id,
                        SenderId = user.Id,
                        Type = EnumNotificationType.AccountUpdated
                    }
                );
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ResponseDTO { Status = 401, Message = ex.Message };
            }
            return new ResponseDTO
            {
                Message = userBanDTO.IsBanned ?  "Profil banni" : "Profil mis à jour",
                Status = 200,
                Data = user.ToUserResponseDTO(),
            };
        }
    }
}
