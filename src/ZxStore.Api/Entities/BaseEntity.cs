namespace ZxStore.Api.Entities;

public abstract class BaseEntity
{
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public DateTime? UpdatedAt { get; set; }
  public Guid? CreatedByUserId { get; set; }
  public Guid? UpdatedByUserId { get; set; }
  public bool IsDeleted { get; set; } = false;
  public DateTime? DeletedAt { get; set; }
}
