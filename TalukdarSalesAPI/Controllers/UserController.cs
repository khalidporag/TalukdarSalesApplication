using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;
using TalukdarSalesAPI.Helpers;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;
using TalukdarSalesAPI.Models.Dto;
using System;
using TalukdarSalesAPI.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace TalukdarSalesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;
        private readonly ApplicationDbContext _authContext;
        public UserController(
            IUserRepository userRepository,
            IUserTypeRepository userTypeRepository,
            ApplicationDbContext authContext)
        {
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
            _authContext = authContext;
        }

        [HttpPost("authenticate")]
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
        public IActionResult RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            //check username
            if (CheckUsernameExistAsync(userObj.Username))
                return BadRequest(new { Message = "Username Already Exist" });

            //var passMessage = CheckPasswordStrength(userObj.Password);
            //if (!string.IsNullOrEmpty(passMessage))
            //    return BadRequest(new { Message = passMessage.ToString() });

            userObj.Password = PasswordHasher.HashPassword(userObj.PhoneNumber);
            userObj.Token = "";
            _userRepository.Add(userObj);
            _userRepository.Commit();
            return Ok(new
            {
                Status = 200,
                Message = "User Added!"
            });
        }

        private bool CheckUsernameExistAsync(string? username)
           => _userRepository.FindBy(x => x.Username == username).Any();

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
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
            var identity = new ClaimsIdentity(new Claim[]
            {
                //new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name,$"{user.Username}")
            });

            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.Now.AddSeconds(10),
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
            var key = Encoding.ASCII.GetBytes("veryverysceret.....");
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
        public ActionResult<User> GetAllUsers(int? userTypeId)
        {
            if (userTypeId != null)
                return Ok(_userRepository.GetAll().Where(n => n.UserTypeId == userTypeId).ToList());
            return Ok(_userRepository.GetAll());
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenApiDto tokenApiDto)
        {
            if (tokenApiDto is null)
                return BadRequest("Invalid Client Request");
            string accessToken = tokenApiDto.AccessToken;
            string refreshToken = tokenApiDto.RefreshToken;
            var principal = GetPrincipleFromExpiredToken(accessToken);
            var username = principal.Identity.Name;
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
    }
}
