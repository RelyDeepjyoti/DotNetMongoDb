using MongoDB.Driver;
using mongodb_dotnet_example.Models;
using System.Linq;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IGamesDatabaseSettings settings)
    {
        //_users = database.GetCollection<User>("Users");
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        _users = database.GetCollection<User>(settings.GamesCollectionName);
    }

    public User Create(User user)
    {
        user.Password = HashPassword(user.Password);
        _users.InsertOne(user);
        return user;
    } 

    public bool Exists(string email)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var user = _users.Find(filter).FirstOrDefault();
        return user != null;
    }
    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        // Verify the plain password against the hashed password
        return BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }

    private string HashPassword(string password)
    {
        // Generate a secure salt and hash the password
        return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
    }
    public User GetByEmail(string email)
    {
        return _users.Find(user => user.Email == email).FirstOrDefault();
    }
    public User GetById(string userId)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        return _users.Find(filter).FirstOrDefault();
    }


}