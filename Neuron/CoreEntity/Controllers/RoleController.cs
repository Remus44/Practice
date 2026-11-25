using System;
using Microsoft.AspNetCore.Mvc;
using Interview.Models;
using Interview.Services;
using System.Collections.ObjectModel;

namespace Interview.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly RoleService roleService;

        public RoleController(RoleService roleService)
        {
            this.roleService = roleService;
        }
        [HttpPost("createUpdateRolesAndPermissions")]
        public IActionResult CreateUpdateRolesAndPermissions([FromBody] CreateUpdateRolesAndPermissionsRequest rolesAndPermissionsRequest)
        {
            try
            {
                Collection<RoleDTO> rolesAndPermissions = roleService.CreateUpdateRolesAndPermissions(rolesAndPermissionsRequest.Roles);
                var response = new CreateRolesAndPermissionsResponse(rolesAndPermissions);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}