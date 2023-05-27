namespace Klab.Toolkit.Common.Entities;

/// <summary>
/// This class is a base class for all entities that need to be audit.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// Gets or sets date time when the entity was created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets user who created the entity. can be empty.
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets date time when the entity was last modified. Can be null.
    /// </summary>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Gets or sets user who last modified the entity. Can be empty.
    /// </summary>
    public string LastModifiedBy { get; set; } = string.Empty;
}
