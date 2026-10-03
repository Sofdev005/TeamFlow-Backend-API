using TeamFlow.Domain.Common;
using TeamFlow.Domain.Exceptions;

namespace TeamFlow.Domain.Entities;

public class Attachment : Entity<Guid>
{
    public Guid TaskId { get; private set; }
    public string FileName { get; private set; } = default!;
    public string BlobUrl { get; private set; } = default!;
    public Guid UploadedByUserId { get; private set; }
    public long SizeBytes { get; private set; }
    public string ContentType { get; private set; } = default!;

    private Attachment() { } // EF Core

    public Attachment(Guid id, Guid taskId, string fileName, string blobUrl, Guid uploadedByUserId, long sizeBytes, string contentType)
        : base(id == Guid.Empty ? Guid.NewGuid() : id)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("FileName is required.");
        if (string.IsNullOrWhiteSpace(blobUrl))
            throw new DomainException("BlobUrl is required.");

        TaskId = taskId;
        FileName = fileName;
        BlobUrl = blobUrl;
        UploadedByUserId = uploadedByUserId;
        SizeBytes = sizeBytes;
        ContentType = contentType;
    }
}