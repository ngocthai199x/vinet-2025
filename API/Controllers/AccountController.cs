using System.Security.Claims;
using API.DTOs;
using API.Extensions;
using Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController(SignInManager<AppUser> singInManager) : BaseApiController
    {
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDto registerDto)
        {
            var user = new AppUser {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.Email,
            };

            var result = await singInManager.UserManager.CreateAsync(user, registerDto.Password);
            if(!result.Succeeded) //return BadRequest(result.Errors);
            {
                // custom modelstate validation instead of using default one of indentity, to make the identity validations comes along with the other fields (eg: firstName, lastName..) 
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }
            
            return Ok();
        }
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await singInManager.SignOutAsync();
            return NoContent();
        }
        // [Authorize]
        [HttpGet("user-info")]
        public async Task<ActionResult> GetUserInfo()
        {
            if (User.Identity?.IsAuthenticated == false) return NoContent();
            var user = await singInManager.UserManager.GetUserByEmailWithAddress(User);
            return Ok(new { user.FirstName, user.LastName, user.Email, Address = user.Address?.ToDto()});
        }
        [HttpGet("auth-status")]
        public ActionResult GetAuthState()
        {
           return Ok(new {IsAuthenticated = User.Identity?.IsAuthenticated?? false});
        }
        [Authorize]
        [HttpPost("address")]
        public async Task<ActionResult<Address>> CreateOrUpdateAddress(AddressDto addresDto)
        {
            var user = await singInManager.UserManager.GetUserByEmailWithAddress(User);
            if(user.Address == null)
            {
                user.Address = addresDto.ToEntity();
            }
            var result = await singInManager.UserManager.UpdateAsync(user);
            if(!result.Succeeded) return BadRequest("Problem updating user address");
            return Ok(user.Address.ToDto());
        } 
    }
}
