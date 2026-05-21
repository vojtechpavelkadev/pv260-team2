using System.ComponentModel.DataAnnotations;

namespace ArkTracker.Infrastructure.Configuration;

public class ArkScraperOptions
{
    [Required(ErrorMessage = "The URL for the Ark Scraper is required.")]
    [Url(ErrorMessage = "The Ark Scraper URL must be a valid URL.")]
    public required string Url { get; init; }
}
