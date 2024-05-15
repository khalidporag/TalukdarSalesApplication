using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalukdarSalesAPI.Context;
using TalukdarSalesAPI.Interfaces;
using TalukdarSalesAPI.Models.Dto;
using TalukdarSalesAPI.Models;

namespace TalukdarSalesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;
        private readonly IApplicationRoleRepository _roleRepository;
        private readonly IApplicationModuleRepository _moduleRepository;
        private readonly ApplicationDbContext _authContext;
        public RoleController(
            IUserRepository userRepository,
            IUserTypeRepository userTypeRepository,
            IApplicationRoleRepository roleRepository,
            IApplicationModuleRepository moduleRepository,
            ApplicationDbContext authContext)
        {
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
            _roleRepository = roleRepository;
            _moduleRepository = moduleRepository;
            _authContext = authContext;
        }

        [HttpPost("createRole")]
        public IActionResult CreateRole([FromBody] ApplicationRole roleObj)
        {
            if (roleObj == null)
                return BadRequest();

            var role = _roleRepository.FindBy(x => x.Name == roleObj.Name).FirstOrDefault();

            if (role != null)
                return NotFound(new { Message = "This Role is already exist. Please try new role.!" });

            _roleRepository.Add(roleObj);
            _roleRepository.Commit();

            return Ok(new
            {
                Status = 200,
                Message = "Role Added!"
            });
        }

        [HttpPost("createModule")]
        public IActionResult CreateModule([FromBody] ApplicationModule moduleObj)
        {
            if (moduleObj == null)
                return BadRequest();

            _moduleRepository.Add(moduleObj);
            _moduleRepository.Commit();

            return Ok(new
            {
                Status = 200,
                Message = "Module Added!"
            });
        }
    }
}
