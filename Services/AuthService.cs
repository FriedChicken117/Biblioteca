using System.Security.Cryptography;
using System.Text;
using Biblioteca.Models;

namespace Biblioteca.Services;

public class AuthService
{
    private readonly XmlDataService _xmlDataService;

    public AuthService(XmlDataService xmlDataService)
    {
        _xmlDataService = xmlDataService;
    }

    public User? ValidateUser(string userName, string password)
    {
        var user = _xmlDataService.GetUserByUserName(userName);
        if (user == null) return null;

        var hash = HashPassword(password);
        return user.PasswordHash == hash ? user : null;
    }

    public void RegisterUser(string userName, string password)
    {
        var user = new User
        {
            UserName = userName,
            PasswordHash = HashPassword(password),
            IsAdmin = false
        };

        _xmlDataService.AddUser(user);
    }

    public static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}

