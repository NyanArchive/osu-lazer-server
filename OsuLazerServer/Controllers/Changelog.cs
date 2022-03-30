using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Octokit;
using OsuLazerServer.Models.Changelog;

namespace OsuLazerServer.Controllers;

[Route("/api/v2/changelog")]
public class Changelog : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var changelogBuildsRaw = Directory.GetFiles(Path.Join("data", "changelog")).Select(c => JsonSerializer.Deserialize<ChangelogBuild>(System.IO.File.ReadAllText(Path.Join(c))));
        var currentUpdateStream = new UpdateStream
        {
            Id = 1,
            Name = "lazer",
            DisplayName = "Lazer",
            IsFeatured = true,
            LatestBuild = null,
        };
        var changelogBuilds = changelogBuildsRaw.Select((c, i) =>
        {
            c.UpdateStream = currentUpdateStream.WithoutBuild();
            c.Versions.Next = changelogBuildsRaw.ElementAtOrDefault(i + 1)?.WithoutStream()??null;
            c.Versions.Previous = changelogBuildsRaw.ElementAtOrDefault(i - 1)?.WithoutStream()??null;
            return c;
        }).ToList();

        currentUpdateStream.LatestBuild = changelogBuilds.OrderByDescending(c => c.CreatedAt).FirstOrDefault();
        currentUpdateStream.LatestBuild.UpdateStream = currentUpdateStream.WithoutBuild();
        

        return Json(new ChangeLogResponse
        {
            Builds = changelogBuilds,
            Streams = new List<UpdateStream>() { currentUpdateStream }
        });
    }

    [HttpGet("{upstream}/{id}")]
    public async Task<IActionResult> GetChangelog([FromRoute(Name = "id")] string id, [FromRoute(Name = "upstream")] string upstream)
    {
        var changelogBuildsRaw = Directory.GetFiles(Path.Join("data", "changelog")).Select(c => JsonSerializer.Deserialize<ChangelogBuild>(System.IO.File.ReadAllText(Path.Join(c))));
        var currentUpdateStream = new UpdateStream
        {
            Id = 1,
            Name = "lazer",
            DisplayName = "Lazer",
            IsFeatured = true,
            LatestBuild = null,
        };
        var changelogBuilds = changelogBuildsRaw.Select((c, i) =>
        {
            c.UpdateStream = currentUpdateStream.WithoutBuild();
            c.Versions.Next = changelogBuildsRaw.ElementAtOrDefault(i + 1)?.WithoutStream()??null;
            c.Versions.Previous = changelogBuildsRaw.ElementAtOrDefault(i - 1)?.WithoutStream()??null;
            return c;
        }).ToList();
        
        return Json(changelogBuilds.FirstOrDefault(c => c.Version == id && c.UpdateStream.Name == upstream));
    }
}