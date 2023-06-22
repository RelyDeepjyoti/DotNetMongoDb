using Microsoft.AspNetCore.Mvc;
using mongodb_dotnet_example.Models;
using mongodb_dotnet_example.Services;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using MongoDB.Bson;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
//using Newtonsoft.Json;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IOptions<AppSettings> _appSettings;

    public UsersController(UserService userService, IOptions<AppSettings> appSettings)
    {
        _userService = userService;
        _appSettings = appSettings;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterModel model)
    {
        // Check if the user already exists in the database
        if (_userService.Exists(model.Email))
        {
            return BadRequest("User already exists");
        }

        // Create a new User object to save in the database
        var user = new User
        {
            Email = model.Email,
            Password = model.Password
        };

        // Save the user in the database
        _userService.Create(user);

        // Example response
        var response = new
        {
            Message = "Registration successful",
            model.Email
        };
        return Ok(response);
    }
    [HttpPost("login")]
    public IActionResult Login(LoginInput loginInput)
    {
        var user = _userService.GetByEmail(loginInput.Email);

        if (user == null || !_userService.VerifyPassword(loginInput.Password, user.Password))
        {
            // Invalid credentials
            return Unauthorized();
        }

        
        // Generate JWT token
        var token = GenerateJwtToken(user);

        // Create an object containing user data and token
        var response = new
        {
            token,
            User = new
            {
               user.Email, 
            }
        };

        // Successful login
        return Ok(response);
    }
    private string GenerateJwtToken(User user)
    {
        //string secretKey = SecretKeyGenerator.GenerateSecretKey();
        string secretKey = _appSettings.Value.JwtSecretKey;
        //Console.WriteLine($"Generated secret key: {secretKey}");
        // Set the secret key used to sign the token
        var key = Encoding.ASCII.GetBytes(secretKey);

        // Create claims for the user
        var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email)
        // Add additional claims as needed
    };

        // Create token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(360), // Set token expiration time
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // Create token handler
        var tokenHandler = new JwtSecurityTokenHandler();

        // Generate token
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // Convert token to string
        var tokenString = tokenHandler.WriteToken(token);

        return tokenString;
    }
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        // Retrieve the token from the request header
        string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        Console.WriteLine(token);
        // Validate the token and retrieve the user ID
        if (!ValidateToken(token, out var userId))
        {
            return Unauthorized();
        }
        Console.WriteLine($"userIDdd {userId}");
        // Retrieve the user from the database
        var user = _userService.GetById(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        // Return the user's profile data
        var profileData = new
        {
             user.Email,
             user.Id,
        };

        return Ok(profileData);
    }

    private bool ValidateToken(string token, out string userId)
    {
        userId = null;
        // Generate the secret key using SecretKeyGenerator.GenerateSecretKey() method
        //string secretKey = SecretKeyGenerator.GenerateSecretKey();
        string secretKey = _appSettings.Value.JwtSecretKey;

        // Create token validation parameters
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Set to true if you want to validate the token issuer
            ValidateAudience = false, // Set to true if you want to validate the token audience
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
        try
        {
            // Validate and extract the user ID from the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value;
            return true;
        }
        catch
        {
            return false;
        }
    }
    [HttpPost("likes")]
    public async Task<IActionResult> CreateLike()
    {
        // Retrieve the token from the request header
        string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        // Validate the token and retrieve the user ID
        if (!ValidateToken(token, out var userId))
        {
            return Unauthorized();
        }

        using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
        {
            // Read the request body asynchronously
            string requestBodyJson = await reader.ReadToEndAsync();

            // Deserialize the request body JSON to get the postId
            dynamic requestBody = JsonConvert.DeserializeObject(requestBodyJson);
            string postId = requestBody.postId;

            // Create a new Like object with the provided data
            var like = new Like
            {
                Id = ObjectId.GenerateNewId().ToString(),
                PostId = postId,
                Posted = DateTime.UtcNow,
                IsLike = true
            };

            // Save the like to the database
            _userService.CreateLikeForUser(userId, like);

            return Ok("Like created successfully");
        }
    }


    [HttpGet("likes")]
    public IActionResult GetLikes()
    {
        string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!ValidateToken(token, out var userId))
        {
            return Unauthorized();
        }

        var user = _userService.GetById(userId);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var profileData = new
        {
            
            user.Email,
            user.Id,
            user.Likes 
        };

        return Ok(profileData);
    }
    [HttpPut("likesUpdate")]
    public   IActionResult UpdateLike(string likeId)
    {
        // Retrieve the token from the request header
        string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        // Validate the token and retrieve the user ID
        if (!ValidateToken(token, out var userId))
        {
            return Unauthorized();
        } 
            var like = new Like
            {
                Id = likeId,
                //PostId = postId,
                Posted = DateTime.UtcNow,
                IsLike = false // set to false
            };
        Console.WriteLine($"likeId {likeId}");
        // Update the like in the database
        _userService.UpdateLikeForUser(userId, like,false);

            return Ok(like);
    }
    [HttpDelete("likesUpdate/{likeId}")]
    public IActionResult DeleteLike([FromRoute]  string likeId)
    {
        // Retrieve the token from the request header
        string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        // Validate the token and retrieve the user ID
        if (!ValidateToken(token, out var userId))
        {
            return Unauthorized();
        }
        var like = new Like
        {
            Id = likeId,
            //PostId = postId,
            Posted = DateTime.UtcNow,
            IsLike = false // set to false
        };
        Console.WriteLine($"likeId {likeId}");
        // Update the like in the database
        _userService.UpdateLikeForUser(userId, like, true);

        return Ok(like);
    }
    [HttpGet("users")]
    public IActionResult GetAllUsers(int page = 1, int pageSize = 10, string sortBy = "id", bool isDescending = false)
    { 
        // Get all users from the database
        var users = _userService.GetAllUsers(page, pageSize,sortBy,isDescending);

        return Ok(users);
    }
    [HttpGet("users/index")]
    public IActionResult GetAllUsersIndex(int page = 1, int pageSize = 10, string sortBy = "id", bool isDescending = false)
    {
        // Get all users from the database
        var users = _userService.GetAllUsersByIndex(page, pageSize, sortBy, isDescending);

        return Ok(users);
    }
}