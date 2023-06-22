using MongoDB.Driver;
using mongodb_dotnet_example.Models;
using System.Linq;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MongoDB.Bson;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class UserService
{
    private readonly IMongoCollection<User> _users;
    private readonly IMongoCollection<UserList> _userList;
    public UserService(IGamesDatabaseSettings settings)
    {
        //_users = database.GetCollection<User>("Users");
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        _users = database.GetCollection<User>(settings.GamesCollectionName);
        _userList = database.GetCollection<UserList>(settings.GamesCollectionName);
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
        //Find(game => game.Id == id).FirstOrDefault()
        return _users.Find(filter).FirstOrDefault();
    }
    public void CreateLikeForUser(string userId, Like like)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Push(u => u.Likes, like);
        _users.UpdateOne(filter, update);
    }
    public void UpdateLikeForUser(string userId, Like like, bool isRemoved)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(user=> user.Id, userId),
            Builders<User>.Filter.Eq("Likes.Id", like.Id)
        );
        var update = Builders<User>.Update
            .Set("Likes.$.Posted", like.Posted)
            .Set("Likes.$.IsLike", like.IsLike);
        if (isRemoved) {
               update = Builders<User>.Update.PullFilter("Likes", Builders<Like>.Filter.Eq(like => like.Id, like.Id));
        }
        
       

        try {
            _users.UpdateOne(filter, update);
        }catch (Exception e) {
            Console.WriteLine(e.Message);
        }
    }
    public List<UserList> GetAllUsers(int page, int pageSize, string sortBy, bool isDescending)
    {
        //var users = _userList.Find(user => true).ToList();
        var skipCount = (page - 1) * pageSize;
        var sortDefinition = Builders<UserList>.Sort.Ascending(sortBy);
        if (isDescending)
        {
            sortDefinition = Builders<UserList>.Sort.Descending(sortBy);
        }

    var users = _userList.Find(user => true)
                            .Sort(sortDefinition)
                            .Skip(skipCount)
                            .Limit(pageSize)
                            .Project(u => new UserList
                      {
                          Id = u.Id,
                          Email = u.Email
                      })
                      .ToList();
        return users;
    }
    public List<UserList> GetAllUsersByIndex(int page, int pageSize, string sortBy, bool isDescending)
    {
        var skipCount = (page - 1) * pageSize;
        var sortDefinition = Builders<UserList>.Sort.Ascending(sortBy);
        if (isDescending)
        {
            sortDefinition = Builders<UserList>.Sort.Descending(sortBy);
        }

        // Create an index definition
        var indexKeysDefinition = Builders<UserList>.IndexKeys.Ascending(sortBy);
        var indexModel = new CreateIndexModel<UserList>(indexKeysDefinition);

        // Create the index on the _userList collection
        _userList.Indexes.CreateOne(indexModel);

        var users = _userList.Find(user => true)
                            .Sort(sortDefinition)
                            .Skip(skipCount)
                            .Limit(pageSize)
                            .Project(u => new UserList
                            {
                                Id = u.Id,
                                Email = u.Email
                            })
                            .ToList();
        return users;
    }

}