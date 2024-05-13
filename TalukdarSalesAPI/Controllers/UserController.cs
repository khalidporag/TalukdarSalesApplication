using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;
        public UserController(
            IUserRepository userRepository,
            IUserTypeRepository userTypeRepository)
        {
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            var user =  _userRepository.FindBy(x => x.Username == userObj.Username && x.Password == userObj.Password).FirstOrDefault();

            if (user == null)
                return NotFound(new { Message = "User not found!" });


            return Ok(new
            {
                Message = "Login Success"
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] User userObj)
        {
            if (userObj == null)
                return BadRequest();

            _userRepository.Add(userObj);
            _userRepository.Commit();


            return Ok(new
            {
                Message = "Register Success"
            });
        }
    }
}
