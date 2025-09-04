using E_CommerceSystem.Models;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static E_CommerceSystem.Models.UserDTO;

using AutoMapper;
using AutoMapper.QueryableExtensions; // <-- needed for ProjectTo
using Microsoft.EntityFrameworkCore;  // <-- for ToListAsync

namespace E_CommerceSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[Controller]")]
    public class UserController: ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IConfiguration configuration, IMapper mapper)
        {
            _userService = userService;
            _configuration = configuration;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserRegisterDto dto)
        {
            var user = _mapper.Map<User>(dto); // Password hashing happens in the service/repo
            _userService.AddUser(user);
            var read = _mapper.Map<UserReadDto>(user);
            return CreatedAtAction(nameof(GetUserById), new { uid = user.UID }, read);
        }


        [AllowAnonymous]
        [HttpGet("Login")]
        public IActionResult Login(string email, string password)
        {
            try
            {
                var user = _userService.GetUSer(email, password);
                string token = GenerateJwtToken(user.UID.ToString(), user.UName, user.Role);
                return Ok(token);

            }
            catch (Exception ex)
            {
                // Return a generic error response
                return StatusCode(500, $"An error occurred while login. {(ex.Message)}");
            }

        }

        [AllowAnonymous]
        [HttpGet("GetAllUsers")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAllUsers();
            var dtos = _mapper.Map<List<UserReadDto>>(users);
            return Ok(dtos);
        }


        [Authorize]
        [HttpGet("GetUserById")]
        public IActionResult GetUserById(int uid)
        {
            var user = _userService.GetUserById(uid);
            var dto = _mapper.Map<UserReadDto>(user);
            return Ok(dto);
        }

        [NonAction]
        public string GenerateJwtToken(string userId, string username, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Name, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //    [NonAction]
        //    public string GenerateJwtToken(string userId, string username, string role)
        //    {
        //        var jwt = _configuration.GetSection("Jwt");
        //        var keyString = jwt["Key"]!;
        //        var issuer = jwt["Issuer"]!;
        //        var audience = jwt["Audience"]!;
        //        var expiryMin = double.Parse(jwt["ExpiryInMinutes"]!);

        //        var claims = new[]
        //        {
        //    new Claim(JwtRegisteredClaimNames.Sub, userId),
        //    new Claim(JwtRegisteredClaimNames.Name, username),
        //    new Claim(ClaimTypes.Role, role),
        //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //};

        //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //        var token = new JwtSecurityToken(
        //            issuer: issuer,
        //            audience: audience,
        //            claims: claims,
        //            expires: DateTime.UtcNow.AddMinutes(expiryMin),
        //            signingCredentials: creds
        //        );

        //        return new JwtSecurityTokenHandler().WriteToken(token);
        //    }



    }
}
