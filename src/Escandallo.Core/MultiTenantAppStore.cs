using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Escandallo.Core;

public static class MultiTenantAppStore
{
    private static readonly ConcurrentDictionary<string, TenantCatalogDto> Centers = new();

    static MultiTenantAppStore() { ResetToSeed(); }

    public static void ResetToSeed()
    {
        Centers.Clear();
        Centers["center-madrid-01"] = MultiTenantSeedCatalog.CreateDefaultCenter("center-madrid-01", "Estética Lumière Madrid");
        Centers["center-barcelona-02"] = MultiTenantSeedCatalog.CreateDefaultCenter("center-barcelona-02", "Belleza Diagonal BCN");
    }

    public static TenantCatalogDto GetCenter(string centerId)
        => Centers.GetOrAdd(centerId, id => MultiTenantSeedCatalog.CreateDefaultCenter(id, $"Centro {id}"));

    public static TenantCatalogDto SaveCenter(TenantCatalogDto updated)
        => Centers[updated.CenterId] = updated;

    public static IReadOnlyList<object> ListCentersSummary()
        => Centers.Values.Select(c => (object)new { c.CenterId, c.CenterName, c.SubscriptionPlan }).ToList();
}
