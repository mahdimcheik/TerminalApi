using System.Security.Claims;
using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface IUsersService
    {
        Task<UserApp?> GetTeacherUser();
        Task<ResponseDTO<List<UserApp>>> GetAllStudentsDTO(QueryPagination query);
        Task<ResponseDTO<UserResponseDTO>> Update(UserUpdateDTO model, ClaimsPrincipal UserPrincipal);
        Task<ResponseDTO<UserResponseDTO>> BanUnbanUser(UserBanDTO userBanDTO);
    }
} 