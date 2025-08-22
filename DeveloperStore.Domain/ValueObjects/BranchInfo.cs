namespace DeveloperStore.Domain.ValueObjects;

/// <summary>
/// External identity for Branch from another bounded context.
/// Contains denormalized data to avoid cross-aggregate references.
/// </summary>
public sealed class BranchInfo : IEquatable<BranchInfo>
{
  public Guid BranchId { get; }
  public string Name { get; }
  public string Location { get; }

  // EF Core constructor
  private BranchInfo()
  {
    BranchId = Guid.Empty;
    Name = string.Empty;
    Location = string.Empty;
  }

  private BranchInfo(Guid branchId, string name, string location)
  {
    if (branchId == Guid.Empty)
      throw new ArgumentException("Branch ID cannot be empty", nameof(branchId));

    if (string.IsNullOrWhiteSpace(name))
      throw new ArgumentException("Branch name cannot be null or empty", nameof(name));

    if (string.IsNullOrWhiteSpace(location))
      throw new ArgumentException("Branch location cannot be null or empty", nameof(location));

    BranchId = branchId;
    Name = name.Trim();
    Location = location.Trim();
  }

  /// <summary>
  /// Create a BranchInfo instance
  /// </summary>
  public static BranchInfo Of(Guid branchId, string name, string location)
      => new(branchId, name, location);

  public bool Equals(BranchInfo? other)
  {
    return other is not null && BranchId == other.BranchId;
  }

  public override bool Equals(object? obj)
  {
    return Equals(obj as BranchInfo);
  }

  public override int GetHashCode()
  {
    return BranchId.GetHashCode();
  }

  public override string ToString()
  {
    return $"{Name} - {Location}";
  }

  public static bool operator ==(BranchInfo? left, BranchInfo? right)
  {
    return left?.Equals(right) ?? right is null;
  }

  public static bool operator !=(BranchInfo? left, BranchInfo? right)
  {
    return !(left == right);
  }
}
