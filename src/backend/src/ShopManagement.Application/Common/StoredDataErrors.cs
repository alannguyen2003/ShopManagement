using ShopManagement.Domain.Common;

namespace ShopManagement.Application.Common;

public static class StoredDataErrors
{
    public static readonly Error UnreadableStoredData = new(
        "StoredData.Unreadable",
        "The stored data cannot be read.");
}
