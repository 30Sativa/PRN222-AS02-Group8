using Microsoft.AspNetCore.Http;
using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IPaymentService
    {
        string CreateVnPayPaymentUrl(Order order, HttpContext context);
        Task<bool> ProcessVnPayCallbackAsync(IQueryCollection collections);
    }
}
