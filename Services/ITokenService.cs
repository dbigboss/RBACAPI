using RBACApi.Models;

namespace RBACApi.Services;

public interface ITokenService
{
    Task<string> GenerateTokenAsync(ApplicationUser user);
}