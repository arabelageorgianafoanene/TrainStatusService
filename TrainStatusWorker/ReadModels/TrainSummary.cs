namespace TrainStatusWorker.ReadModels
{
    public record TrainSummary
    {
        public Guid TrainId { get; init; }
        public required string CurrentStatus { get; set; }
        public DateTime LastUpdatedAtUtc { get; set; }
    }

}
