using AuthAPI.Context;
using AuthAPI.DTO;
using AuthAPI.Helpers;
using AuthAPI.Model;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AuthAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        // first we will inject the dbcontext do get the databse infos
        // creating a constructor
        // Declares a private, read-only field to store the injected DbContext instance.
        // Ensures the field is immutable after being assigned in the constructor.
        private readonly AddDbContext _dbContext;
        // automapper 
        private readonly IMapper _mapper; // Inject AutoMapper 

        public UserController(AddDbContext addDbContext, IMapper mapper)
        {
            // Dependency Injection: ASP.NET Core provides an instance of AddDbContext.
            // Assigning the injected DbContext to the private field for use in the controller.
            _dbContext = addDbContext;
            _mapper = mapper; // assign injected Automapper 
            
        }
        // HttpPost: Submits new data to the server. Typically used to create new resources.
        [HttpPost("authentication")]
        //The return type indicates an asynchronous method that will eventually produce an IActionResult.
        //IActionResult represents the HTTP response returned by the controller action, such as Ok(), BadRequest(), or NotFound().
        //[FromBody]: Specifies that the method should bind the incoming HTTP request body to the userobj parameter.
        //Marks the method as asynchronous, enabling non-blocking execution for operations like database queries or external API calls.
        //Improves performance by freeing up server resources while waiting for tasks to complete.
        public async Task<IActionResult> Authentication([FromBody] LoginDTO userDto)
        {
            // if the user is null then return badrequest
            if(userDto == null)
            {
                return BadRequest();
            }
            // create a variable for user to compaare the values and let it know about if the user is found or not and if found then login successfull
            //The await keyword pauses the execution of an asynchronous method until the awaited task completes, allowing other tasks to run in the meantime. It ensures non-blocking code execution, resuming the method once the task is finished.Keeps the application responsive (e.g., in a web or UI application).

            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userDto.UserName );
            if(user == null)
                return NotFound(new {Message = "user not found"});

            // check  for the passwoord as it is in encrypted way 
            if(!PasswordHasher.VerifyPassword(userDto.Password, user.Password)){
                return BadRequest(new {Message = " Password is Incorrect"});
            }

            // Map the User entity to UserDto 
            // AutoMapper is a library that simplifies the process of object-to - object mapping, enabling you to automatically transfer data between different object types, such as from DTOs to entities.It reduces the need for manual mapping code by configuring mappings once, improving code maintainability and readability.
            
            var userResponse = _mapper.Map<LoginDTO>(user);

            // Create jwt token 
            user.Token = CreateJwt(user);
            // if found then return login success
            return Ok(new
            {
                Token = user.Token,
                Message = "Login Success!",
                   // User = userResponse
            });
            }

        [HttpPost("register")]
        public async Task<IActionResult> register([FromBody] RegisterUserDTO userDto)
        {
            if(userDto == null)
            {
                return BadRequest();
            }
          
            //check username
            if (await CheckUserNameAsync(userDto.UserName)) 
                return BadRequest(new {Message = " Username Already Exist! "});

            //check email
            if (await CheckEmailAsync(userDto.Email))
                return BadRequest(new { Message = "Email Already Exist!" });

            //check password 
            var pass = CheckPasswordStrength(userDto.Password);
            if(!string.IsNullOrEmpty(pass))
                return BadRequest(new {Message = pass.ToString()});
            // map RegisterDTO to user entity 
            var user = _mapper.Map<User>(userDto);
            // use the password hasher for hashing the password 
            user.Password = PasswordHasher.HashPassword(userDto.Password);
            user.Role = "User";
            user.Token = "";

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return Ok(new
            {
                Message = "User Registered!"
            }); ;
        }

        private Task<bool> CheckUserNameAsync(string userName)
            => _dbContext.Users.AnyAsync(x => x.UserName == userName);

        private Task<bool> CheckEmailAsync(string email)
            => _dbContext.Users.AnyAsync(x => x.Email == email);

        private string CheckPasswordStrength(string password)
        {
            StringBuilder sb = new StringBuilder();
            // Check minimum password length
            if (password.Length < 8)
            { sb.Append("Minimum password length should be 8 characters." + Environment.NewLine); }
            // Check for alphanumeric condition (letters and digits)
            if (!(Regex.IsMatch(password, "[a-z]") && Regex.IsMatch(password, "[A-Z]") && Regex.IsMatch(password, "[0-9]")))
            { sb.Append("Password should contain at least one lowercase letter, one uppercase letter, and one number." + Environment.NewLine);}
            // Check for special characters
            if (!Regex.IsMatch(password, "[<>{}/[\\]=+)(*&^%$#@!~`.:;,_]"))
            { sb.Append("Password should contain at least one special character such as <,>,?,/, {,}, [, =, +, (, *, &, ^, %, $, #, @, !, ~, `, ., :, ;, _, etc." + Environment.NewLine); }
            return sb.ToString();
        }

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("veryverysceretveryverysceretveryverysceret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name,$"{user.FirstName}:{user.LastName}")
            });
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        // get all the user 
        [HttpGet]
        public async Task<ActionResult<User>> GetAllUsers()
        {
            return Ok(await _dbContext.Users.ToListAsync());
        }
    }
}

