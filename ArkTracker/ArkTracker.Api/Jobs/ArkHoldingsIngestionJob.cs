using ArkTracker.Application.Interfaces;
using Quartz;

namespace ArkTracker.Api.Jobs;

public sealed class ArkHoldingsIngestionJob(
    IArkScraperService arkScraperService,
    IHoldingRepository holdingRepository,
    ILogger<ArkHoldingsIngestionJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Starting ARKK holdings ingestion job.");

        try
        {
            IEnumerable<ArkTracker.Domain.Entities.HoldingRecord> records = await arkScraperService.DownloadHoldingsAsync();
            List<ArkTracker.Domain.Entities.HoldingRecord> list = records.ToList();

            await holdingRepository.AddRangeAsync(list);
            logger.LogInformation("ARKK holdings ingestion job saved {Count} records.", list.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during the ARKK holdings ingestion job.");
            throw new JobExecutionException(ex, refireImmediately: false);
        }
    }
}

