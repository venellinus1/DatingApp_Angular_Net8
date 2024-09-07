using API.Controllers;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace API;

public class BuggyController(DataContext context)
 : BaseApiController
{
    [HttpGet("auth")]
    public ActionResult<string> GetAuth()
    {
        return "secret text";
    }

    [HttpGet("bad-request")]
    public ActionResult<string> GetBadRequest()
    {
        return BadRequest("this was not a good request");
    }

    [HttpGet("server-error")]
    public ActionResult<AppUser> GetServerError()
    {
        var thing = context.Users.Find(-1) ?? throw new Exception(" a bad thing has happened");
        
        return thing;
    }

    [HttpGet("not-found")]
    public ActionResult<AppUser> GetNotFound()
    {
        var thing = context.Users.Find(-1);
        if (thing == null)
            return NotFound();

        return thing;
    }
}