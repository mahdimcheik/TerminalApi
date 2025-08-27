using TerminalApi.Models;

namespace TerminalApi.Interfaces
{
    public interface IPdfService
    {
        Task<byte[]> GeneratePdfAsync(OrderResponseForStudentDTO order);
        Task<byte[]> GenerateQuestPdfAsync(OrderResponseForStudentDTO order);
        
    }
} 