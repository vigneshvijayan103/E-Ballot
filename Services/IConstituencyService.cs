using EBallotApi.Dto;
using EBallotApi.Models;
namespace EBallotApi.Services
{
    public interface IConstituencyService
    {
        Task<int> AddConstituencyAsync(ConstituencyDto dto, int updatedByAdminId);

        Task<IEnumerable<ConstituencyResponseDto>> GetAllConstituenciesAsync();

        Task<ConstituencyResponseDto?> GetConstituencyByIdAsync(int id);

         Task<bool> UpdateConstituencyAsync(UpdateConstituencyDto dto, int updatedByAdminId);

    }
}
