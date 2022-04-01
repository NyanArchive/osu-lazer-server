using Microsoft.AspNetCore.Mvc;
using OsuLazerServer.Database;
using OsuLazerServer.Models;
using OsuLazerServer.Models.Response.Comments;
using OsuLazerServer.Services.Users;

namespace OsuLazerServer.Controllers;

[ApiController]
[Route("/api/v2/")]
public class CommentsController : Controller
{

    private IUserStorage _storage;
    private LazerContext _context;
    public CommentsController(LazerContext ctx, IUserStorage storage)
    {
        _context = ctx;
        _storage = storage;
    }

    [HttpGet("comments")]
    public async Task<IActionResult> Index()
    {
        return Json(new CommentsResponse
        {
            Comments = new List<Comment>(),
            Total = 0,
            Users = new List<APIUser>(),
            HasMore = false,
            IncludedComments = new List<Comment>(),
            PinnedComments = new List<Comment>(),
            UserFollow = false,
            UserVotes = new List<long>(),
            HasMoreId = 0,
            TopLevelCount = 0,
            Cursor = new APICursor
            {
                Id = 1,
                CreatedAt = DateTime.Today
            },
            Meta = new APICommentableMeta[]
            {
            }
        });
    }
}