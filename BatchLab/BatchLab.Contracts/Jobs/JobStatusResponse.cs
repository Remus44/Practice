namespace BatchLab.Contracts.Jobs;

public record JobStatusResponse(
    string JobId,
    string Status
);