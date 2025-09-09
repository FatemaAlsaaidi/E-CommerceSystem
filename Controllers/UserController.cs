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

using E_CommerceSystem.Repositories;


namespace E_CommerceSystem.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class UserController: ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IAuthService _auth;
        private readonly IRefreshTokenRepo _refreshTokenRepo;

        public UserController(IUserService userService, IConfiguration configuration, IMapper mapper, IRefreshTokenRepo refreshTokenRepo, IAuthService  authService)
        {
            _userService = userService;
            _configuration = configuration;
            _mapper = mapper;
            _refreshTokenRepo = refreshTokenRepo;
            _auth = authService;
        }


        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserRegisterDto dto)
        {
            var user = _mapper.Map<User>(dto); // Password is ignored in the current profile
            user.Password = dto.Password;      // provide it explicitly for hashing
            user.Role ??= "Customer";
            user.CreatedAt = DateTime.UtcNow;
            _userService.AddUser(user);
            var read = _mapper.Map<UserReadDto>(user);
            return CreatedAtAction(nameof(GetUserById), new { uid = user.UID }, read);

        }


        //[AllowAnonymous]
        //[HttpGet("Login")]
        //public IActionResult Login(string email, string password)
        //{
        //    try
        //    {
        //        var user = _userService.GetUSer(email, password);
        //        string token = GenerateJwtToken(user.UID.ToString(), user.UName, user.Role);
        //               return Ok(token);
        //               // issue access token (short) + refresh token (long)
        //        var access = _auth.CreateAccessToken(user);
        //        var(refreshRaw, refreshHash, refreshExp) = _auth.CreateRefreshToken();
        //        _refreshTokenRepo.Add(new RefreshToken
        //        {
        //            UID = user.UID,
        //            TokenHash = refreshHash,
        //            ExpiresAtUtc = refreshExp,
        //            CreatedAtUtc = DateTime.UtcNow
        //             });
        //        _refreshTokenRepo.Save();
                
        //        Response.Cookies.Append("access_token", access, AuthCookieOptions(DateTime.UtcNow.AddMinutes(
        //        double.Parse(_configuration.GetSection("JwtSettings")["ExpiryInMinutes"]!)
        //               )));
        //        Response.Cookies.Append("refresh_token", refreshRaw, AuthCookieOptions(refreshExp));
                
        //              // return a small payload (or nothing)
        //               return Ok(new { message = "Logged in", user = new { user.UID, user.UName, user.Email, user.Role } });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"An error occurred while login. {(ex.Message)}");
        //    }
        //}


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

        // ======================= Helpers =======================
        private static CookieOptions AuthCookieOptions(DateTime? expiresUtc = null)
        {
            return new CookieOptions
            {
                HttpOnly = true,           // لا تُقرأ من JS
                Secure = true,             // يتطلب HTTPS
                SameSite = SameSiteMode.None, // للسماح من Origins مختلفة (Frontend خارجي)
                Expires = expiresUtc
            };
        }

        // ======================= LOGIN =========================
    
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] UserLoginDto dto)
        {
            try
            {
                var user = _userService.GetUSer(dto.Email, dto.Password);

                // 1) create Access + Refresh
                var access = _auth.CreateAccessToken(user);
                var (refreshRaw, refreshHash, refreshExp) = _auth.CreateRefreshToken();

                _refreshTokenRepo.Add(new RefreshToken
                {
                    UID = user.UID,
                    TokenHash = refreshHash,
                    ExpiresAtUtc = refreshExp,
                    CreatedAtUtc = DateTime.UtcNow
                });
                _refreshTokenRepo.Save();

                // 2) Saved in cookies
                var accessExp = DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration.GetSection("JwtSettings")["ExpiryInMinutes"]!)
                );
                Response.Cookies.Append("access_token", access, AuthCookieOptions(accessExp));
                Response.Cookies.Append("refresh_token", refreshRaw, AuthCookieOptions(refreshExp));

                // 3) Also return the token in the body (for use with Swagger/Postman)
                return Ok(new
                {
                    message = "Logged in",
                    access_token = "Token is " + access,
                    //refresh_token = refreshRaw,
                    expires_at = accessExp,
                    user = new { user.UID, user.UName, user.Email, user.Role }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while login. {ex.Message}");
            }
        }


        public sealed class UserLoginDto
        {
            public string Email { get; set; } = default!;
            public string Password { get; set; } = default!;
        }

        // ======================= REFRESH =======================
       
        [Authorize]
        [HttpPost("Refresh Token")]
        public IActionResult RefreshToken()
        {
            var refreshRaw = Request.Cookies["refresh_token"];
            if (string.IsNullOrWhiteSpace(refreshRaw))
                return Unauthorized("Missing refresh token.");

            var oldHash = _auth.Sha256(refreshRaw);
            var old = _refreshTokenRepo.GetByHash(oldHash);
            if (old is null || !old.IsActive)
                return Unauthorized("Invalid refresh token.");

            _auth.RotateRefreshToken(old.UID, refreshRaw, out var newRaw, out var newExp);

            var user = _userService.GetUserById(old.UID);
            var newAccess = _auth.CreateAccessToken(user);

            var accessExp = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration.GetSection("JwtSettings")["ExpiryInMinutes"]!)
            );

            Response.Cookies.Append("access_token", newAccess, AuthCookieOptions(accessExp));
            Response.Cookies.Append("refresh_token", newRaw, AuthCookieOptions(newExp));

        
            return Ok(new
            {
                message = "Refreshed",
                access_token = "New Token Is " + newAccess,
                //refresh_token = newRaw,
                //expires_at = accessExp,
                //user = new { user.UID, user.UName, user.Email, user.Role }
            });
        }


        // ======================= LOGOUT ========================
        // This disables the refresh function and deletes cookies from the browser.
        [Authorize]
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            var refreshRaw = Request.Cookies["refresh_token"];
            if (!string.IsNullOrWhiteSpace(refreshRaw))
                _auth.RevokeRefreshToken(refreshRaw);

            // Remove cookies from the browser
            Response.Cookies.Delete("access_token", AuthCookieOptions());
            Response.Cookies.Delete("refresh_token", AuthCookieOptions());

            return Ok(new { message = "Logged out" });
        }







        //----------------

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

       


    }
}
