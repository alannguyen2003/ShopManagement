using ShopManagement.Domain.Common;

namespace ShopManagement.Application.Common;

public static class StoredDataErrors
{
    public static readonly Error ConnectionStringMissing = new(
        "StoredData.ConnectionStringMissing",
        "ConnectionStrings:Default is missing. Set it in the environment or user secrets.");

    public static readonly Error StoredDataLocked = new(
        "StoredData.Locked",
        "The stored data is locked by a start that did not finish. If no other start is running, delete the __EFMigrationsLock table from the database.");

    public static readonly Error UnreadableStoredData = new(
        "StoredData.Unreadable",
        "The stored data cannot be read.");
}
