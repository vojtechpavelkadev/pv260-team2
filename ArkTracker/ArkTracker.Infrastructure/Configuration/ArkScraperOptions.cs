using System.ComponentModel.DataAnnotations;

namespace ArkTracker.Infrastructure.Configuration;

public class ArkScraperOptions
{
    [Required(ErrorMessage = "ArkScraper URL is required.")]
    [Url(ErrorMessage = "ArkScraper URL must be a valid URL.")]
    public string Url { get; set; } = string.Empty;
}
