namespace VictoriaIdentityProvider.Application.Interfaces.TokenInterfaces
{
    public interface IDefaultTokenService
    {
        (string, int expirationDays) GenerateRandomToken();
        string GetHashFromToken(string token);
    }
}