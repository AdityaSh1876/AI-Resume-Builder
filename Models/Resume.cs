using System.ComponentModel.DataAnnotations;

namespace AIResumeBuilder.Models;

public class Resume
{
    public int Id { get; set; }
    [Required, StringLength(120)]
    public string Title { get; set; } = "My Professional Resume";
    [Required, StringLength(160)]
    public string FullName { get; set; } = "";
    [EmailAddress, StringLength(200)]
    public string Email { get; set; } = "";
    [StringLength(40)]
    public string Phone { get; set; } = "";
    [StringLength(200)]
    public string Location { get; set; } = "";
    [StringLength(300)]
    public string LinkedInUrl { get; set; } = "";
    [StringLength(300)]
    public string PortfolioUrl { get; set; } = "";
    public string Summary { get; set; } = "";
    public string Education { get; set; } = "";
    public string Experience { get; set; } = "";
    public string Projects { get; set; } = "";
    public string Skills { get; set; } = "";
    public string Certifications { get; set; } = "";
    [Required]
    public string UserId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
