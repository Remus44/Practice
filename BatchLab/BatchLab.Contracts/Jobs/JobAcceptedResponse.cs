namespace BatchLab.Contracts.Jobs;

public record JobAcceptedResponse(
    string JobId,
    Guid OperationId
);