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



}
