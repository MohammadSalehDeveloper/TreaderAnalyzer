namespace Core.Domain.Common;

public abstract class AuditableEntity : Entity
{
    public DateTime CreatedAtUtc { get; protected set; }
    public DateTime? UpdatedAtUtc { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAtUtc { get; protected set; }

    protected void MarkCreated(DateTime? utcNow = null)
    {
        CreatedAtUtc = utcNow ?? DateTime.UtcNow;
    }

    protected void MarkUpdated(DateTime? utcNow = null)
    {
        UpdatedAtUtc = utcNow ?? DateTime.UtcNow;
    }

    protected void MarkDeleted(DateTime? utcNow = null)
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAtUtc = utcNow ?? DateTime.UtcNow;
        MarkUpdated(DeletedAtUtc);
    }
}
