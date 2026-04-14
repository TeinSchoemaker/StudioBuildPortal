using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudioBuildPortal.Data;
using StudioBuildPortal.Models;

namespace StudioBuildPortal.Controllers;

public class ProjectsController : Controller
{
    private readonly AppDbContext _db;
    private readonly JenkinsService _jenkins;

    public ProjectsController(AppDbContext db, JenkinsService jenkins)
    {
        _db = db;
        _jenkins = jenkins;
    }

    public async Task<IActionResult> Index()
    {
        var projects = await _db.Projects.OrderBy(p => p.Name).ToListAsync();
        return View(projects);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProjectEntry project)
    {
        if (!ModelState.IsValid)
            return View(project);

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> TriggerBuild(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound();

        await _jenkins.TriggerBuildAsync(project.JenkinsJobName, project.Branch);

        project.LastBuildStatus = "Queued";
        project.LastBuildTimeUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> RefreshStatus(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project == null) return NotFound();

        var (status, buildNumber) = await _jenkins.GetLastBuildAsync(project.JenkinsJobName);

        project.LastBuildStatus = status;
        project.LastBuildNumber = buildNumber;
        project.LastBuildTimeUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}