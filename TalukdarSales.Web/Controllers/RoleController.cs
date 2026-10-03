using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalukdarSales.Web.Context;
using TalukdarSales.Web.Interfaces;
using TalukdarSales.Web.Models.Dto;
using TalukdarSales.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace TalukdarSales.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserTypeRepository _userTypeRepository;
        private readonly IApplicationRoleRepository _roleRepository;
        private readonly IRoleWisePermissionRepository _roleWisePermissionRepository;
        private readonly IApplicationModuleRepository _moduleRepository;
        private readonly ApplicationDbContext _authContext;
        public RoleController(
            IUserRepository userRepository,
            IUserTypeRepository userTypeRepository,
            IApplicationRoleRepository roleRepository,
            IRoleWisePermissionRepository roleWisePermissionRepository,
            IApplicationModuleRepository moduleRepository,
            ApplicationDbContext authContext)
        {
            _userRepository = userRepository;
            _userTypeRepository = userTypeRepository;
            _roleRepository = roleRepository;
            _roleWisePermissionRepository = roleWisePermissionRepository;
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

        [HttpPost("addRoleWiseModule")]
        public IActionResult AddRoleWiseModule([FromBody] RoleWiseModuleDto roleWiseModuleObj)
        {
            if (roleWiseModuleObj == null)
                return BadRequest();
            var roleWisePermissonObjList = new List<RoleWisePermission>();
            foreach(var moduleId in roleWiseModuleObj.ModuleIds)
            {
                var obj = new RoleWisePermission();
                obj.RoleId = roleWiseModuleObj.RoleId;
                obj.ModuleId = moduleId;

                roleWisePermissonObjList.Add(obj);
            }

            if(roleWisePermissonObjList.Count > 0)
            {
                _roleWisePermissionRepository.AddRange(roleWisePermissonObjList);
                _roleWisePermissionRepository.Commit();
            }
            return Ok(new
            {
                Status = 200,
                Message = "Module Added to Role!"
            });
        }

        [HttpGet("getAllRoles")]
        public ActionResult<ApplicationRole> GetAllRoles()
        {
            return Ok(_roleRepository.GetAll());
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

        [HttpGet("getAllModules")]
        public ActionResult<ApplicationModule> GetAllModules()
        {
            return Ok(_moduleRepository.GetAll());
        }
    }
}
