namespace CarAuction.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateAccessToken(int userId, string email, string? role);
    string GenerateRefreshToken();
}
