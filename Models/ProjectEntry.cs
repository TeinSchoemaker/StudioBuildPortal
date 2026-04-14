using System.ComponentModel.DataAnnotations;

namespace StudioBuildPortal.Models;

public class ProjectEntry
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string RepoUrl { get; set; } = "";

    [Required]
    public string JenkinsJobName { get; set; } = "";

    [Required]
    public string Branch { get; set; } = "main";

    public string LastBuildStatus { get; set; } = "Never Run";
    public int? LastBuildNumber { get; set; }
    public DateTime? LastBuildTimeUtc { get; set; }
}