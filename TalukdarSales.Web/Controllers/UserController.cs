using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using TalukdarSales.Web.Helpers;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models;
using TalukdarSales.Web.Models.Dto;
using TalukdarSales.Web.Services;
using System;
using TalukdarSales.Web.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;

namespace TalukdarSales.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [EnableCors("MyPolicy")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;
        private readonly IUserRoleMappingRepository _userRoleMappingRepository;

        private readonly ApplicationDbContext _authContext;
        private readonly IConfiguration _configuration;
        private readonly UserService _userService;
        public UserController(
            IUserRepository userRepository,
            IUserTypeRepository userTypeRepository,
            IUserRoleMappingRepository userRoleMappingRepository,
            ApplicationDbContext authContext,
            IConfiguration configuration,
            UserService userService)
        {
            _userService = userService;
            _configuration = configuration;
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
            _userRoleMappingRepository = userRoleMappingRepository;
            _authContext = authContext;
        }

        [HttpPost("authenticate")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public IActionResult Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user = _userRepository.FindBy(x => x.Username == userObj.Username).FirstOrDefault();

            if (user == null)
                return NotFound(new { Message = "User not found!" });


            if (!PasswordHasher.VerifyPassword(userObj.Password, user.Password))
            {
                return BadRequest(new { Message = "Password is Incorrect" });
            }

            if (PasswordHasher.NeedsRehash(user.Password))
                user.Password = PasswordHasher.HashPassword(userObj.Password);

            user.Token = CreateJwt(user);
            var newAccessToken = user.Token;
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(5);
            _userRepository.Commit();

            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromForm] CreateUserDto input)
        {
            var (ok, message) = await _userService.CreateAsync(input);
            if (!ok)
                return BadRequest(new { Message = message });
            return Ok(new { Status = 200, Message = message });
        }

        [HttpPost("updateUser")]
        public IActionResult UpdateUser([FromForm] UpdateUserDto input)
        {
            if (input == null || !_userService.Update(input))
                return BadRequest();
            return Ok(new { Status = 200, Message = "User Updated!" });
        }

        //private static string CheckPasswordStrength(string pass)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    if (pass.Length < 9)
        //        sb.Append("Minimum password length should be 8" + Environment.NewLine);
        //    if (!(Regex.IsMatch(pass, "[a-z]") && Regex.IsMatch(pass, "[A-Z]") && Regex.IsMatch(pass, "[0-9]")))
        //        sb.Append("Password should be AlphaNumeric" + Environment.NewLine);
        //    if (!Regex.IsMatch(pass, "[<,>,@,!,#,$,%,^,&,*,(,),_,+,\\[,\\],{,},?,:,;,|,',\\,.,/,~,`,-,=]"))
        //        sb.Append("Password should contain special charcter" + Environment.NewLine);
        //    return sb.ToString();
        //}

        private string CreateJwt(User user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var identity = new ClaimsIdentity(new Claim[]
            {
                                new Claim(ClaimTypes.Name,$"{user.Username}")
            });

            var roleIds = _userRoleMappingRepository.FindBy(m => m.UserId == user.Id).Select(m => m.RoleId).ToList();
            foreach (var role in _authContext.ApplicationRoles.Where(r => roleIds.Contains(r.Id)).ToList())
                identity.AddClaim(new Claim(ClaimTypes.Role, role.Name));

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.AddMinutes(_configuration.GetValue("Jwt:AccessTokenMinutes", 30)),
                SigningCredentials = credentials
            };
            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            return jwtTokenHandler.WriteToken(token);
        }

        private string CreateRefreshToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var refreshToken = Convert.ToBase64String(tokenBytes);

            var tokenInUser = _authContext.Users
                .Any(a => a.RefreshToken == refreshToken);
            if (tokenInUser)
            {
                return CreateRefreshToken();
            }
            return refreshToken;
        }

        private ClaimsPrincipal GetPrincipleFromExpiredToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = false
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("This is Invalid Token");
            return principal;

        }

        [HttpGet]
        public ActionResult<List<UserDto>> GetAllUsers(int? userTypeId, string name)
            => Ok(_userService.Search(userTypeId, name));

        [HttpPost("refresh")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
                return BadRequest("Invalid Client Request");
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            ClaimsPrincipal principal;
            try
            {
                principal = GetPrincipleFromExpiredToken(accessToken);
            }
            catch (Exception)
            {
                return BadRequest("Invalid Request");
            }
            var username = principal.Identity?.Name;
            var user = _userRepository.FindBy(u => u.Username == username).FirstOrDefault();
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return BadRequest("Invalid Request");
            var newAccessToken = CreateJwt(user);
            var newRefreshToken = CreateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _authContext.SaveChangesAsync();
            return Ok(new TokenApiDto()
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
            });
        }


        [HttpPost("userType")]
        public IActionResult UserType([FromBody] UserType userTypeObj)
        {
            if (userTypeObj == null)
                return BadRequest();

            _userTypeRepository.Add(userTypeObj);
            _userTypeRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "User Type Added!"
            });
        }


        [HttpGet("getAllUserTypes")]
        public ActionResult<UserType> GetAllUserTypes()
        {
            return Ok(_userTypeRepository.GetAll());
        }

        [HttpGet("getLoggedInUser")]
        public ActionResult<string> GetLoggedInUser(string username)
        {
            var user = _userRepository.FindBy(x => x.Username == username)?.FirstOrDefault();
            var userName = "";
            
            if (user != null)
            {
                userName = user.FirstName + " " + user.LastName;
            }

            return userName != ""? userName: null;
        }
    }
}
