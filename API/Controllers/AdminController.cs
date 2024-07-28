using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AdminController : BaseApiController
    {

        private UserManager<AppUser> _userManager;
        public AdminController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("users-with-roles")]
        public async Task<ActionResult> GetUsersWithRoles()
        {
            var users = await _userManager.Users
                .OrderBy(x => x.UserName)
                .Select(x => new
                {
                    x.Id,
                    Username = x.UserName,
                    Roles = x.UserRoles.Select(r => r.Role.Name).ToList()
                }).ToListAsync();
            return Ok(users);
        }

        [Authorize(Policy ="RequireAdminRole")]
        [HttpPost("edit-roles/{username}")]
        public async Task<ActionResult> EditRoles(string username, string roles)
        {
            if (string.IsNullOrEmpty(roles)) return BadRequest("you must select at leats one role");
            var selectedRoles = roles.Split(',').ToArray();

            var user = await _userManager.FindByNameAsync(username);

            var userRoles = await _userManager.GetRolesAsync(user);

            var result = await _userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));

            if (!result.Succeeded) return BadRequest("Failed to add to roles");

            result = await _userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));

            if (!result.Succeeded) return BadRequest("Failed to remove from roles");

            return Ok(await _userManager.GetRolesAsync(user));
        }

        [Authorize(Policy = "ModeratePhotoRole")]
        [HttpGet("photos-to-moderate")]
        public ActionResult GetPhotosFormModeration()
        {
            return Ok("Admins or moderators can see this");
        }
    }
}
