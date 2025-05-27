using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.RequestModel;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest userRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userService.RegisterUserAsync(userRequest);
            if (!result)
            {
                return Conflict(new { message = "Username or Email already exists." });
            }

            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            var token = await _userService.LoginAsync(loginRequest.Username, loginRequest.Password);
            if (token == null)
                return Unauthorized(new { message = "Invalid username or password." });

            return Ok(new { token });
        }
    }
}
