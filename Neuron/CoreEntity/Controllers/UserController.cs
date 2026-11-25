using System;
using Microsoft.AspNetCore.Mvc;
using Interview.Models;
using Interview.Services;
using Newtonsoft.Json;

namespace Interview.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;

    private readonly UserService _userService;


    public UserController(UserService userService, ILogger<UserController> logger)
    {
        this._userService = userService;
        this._logger = logger;
    }

    [HttpPost("createUser")]
    public IActionResult CreateUser([FromBody] CreateUserRequest user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("CreateUser");
            UserDTO userDTO = _userService.CreateUser(user.Name, user.Email, user.RoleCodes);
            var response = new CreateUserResponse(userDTO);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:int}/getUser")]
    public IActionResult GetUser(int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("GetUser");
            UserDTO userDTO = _userService.GetUser(id);
            var response = new CreateUserResponse(userDTO);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("updateUser")]
    public IActionResult UpdateUser([FromBody] UpdateUserRequest user)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            _logger.LogInformation("UpdateUser");
            var userExists = _userService.UpdateUser(user);
            return Ok("User updated!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);
            return BadRequest(new { error = ex.Message });
        }
    }


    [HttpDelete("{id:int}/deleteUser")]
    public IActionResult DeleteUser(int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            _logger.LogInformation("DeleteUser");
            var userExists = _userService.GetUser(id);

            if (userExists == null)
            {
                return NotFound();
            }
            _userService.DeleteUserAsync(userExists);

            return Ok("User deleted!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);
            return BadRequest(new { error = ex.Message });
        }
    }
}
