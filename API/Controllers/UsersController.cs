using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
public class UsersController : BaseApiController
{
    private readonly DataContext _context;
    public UsersController(DataContext context)
    {
        _context = context;
    }
    [AllowAnonymous]
    [HttpGet]
    public ActionResult<IEnumerable<AppUser>> GetUsers()
    {
        var users = _context.Users.ToList();
        return users;
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public ActionResult<AppUser> GetUser(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) return NotFound();
        return user;
    }
}
