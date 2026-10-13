using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Escandallo.Core;

public static class MultiTenantAppStore
{
    public static readonly ConcurrentDictionary<string, TenantCatalogDto> Centers = new(StringComparer.OrdinalIgnoreCase);
    public static readonly ConcurrentDictionary<string, UserAccountDto> Users = new(StringComparer.OrdinalIgnoreCase);
    public static readonly ConcurrentDictionary<string, string> Sessions = new(StringComparer.Ordinal);

    static MultiTenantAppStore() { ResetToSeed(); }

    public static void ResetToSeed() {
        Centers.Clear(); Users.Clear(); Sessions.Clear();
        Users["cliente-a@estetica.es"] = new("cliente-a@estetica.es", "Dirección Estética Lumière (Cliente A)", "Estetica2026!", "ACT-ESTHETIC-USER-001");
        Users["cliente-b@estetica.es"] = new("cliente-b@estetica.es", "Dirección Belleza Sur (Cliente B)", "Estetica2026!", "ACT-ESTHETIC-USER-001");
        Users["ops@escandallo.es"] = new("ops@escandallo.es", "Equipo de Soporte Técnico", "Soporte2026!", "ACT-MAINTAINER-001");
        Centers["center-madrid-01"] = MultiTenantSeedCatalog.CreateDefaultCenter("center-madrid-01", "Centro Madrid — Estética Lumière", "cliente-a@estetica.es");
        Centers["center-valencia-02"] = MultiTenantSeedCatalog.CreateDefaultCenter("center-valencia-02", "Centro Valencia — Estética Levante", "cliente-a@estetica.es");
        Centers["center-sevilla-03"] = MultiTenantSeedCatalog.CreateDefaultCenter("center-sevilla-03", "Centro Sevilla — Belleza Sur", "cliente-b@estetica.es");
    }

    public static TenantCatalogDto GetCenter(string centerId, string ownerId = "cliente-a@estetica.es") {
        return Centers.GetOrAdd(centerId, id => MultiTenantSeedCatalog.CreateDefaultCenter(id, $"Centro {id}", ownerId));
    }

    public static TenantCatalogDto SaveCenter(TenantCatalogDto updated) {
        return Centers[updated.CenterId] = updated;
    }

    public static IReadOnlyList<object> ListCentersSummary(string? ownerEmail = null) {
        var query = string.IsNullOrWhiteSpace(ownerEmail) ? Centers.Values : Centers.Values.Where(c => string.Equals(c.OwnerId, ownerEmail, StringComparison.OrdinalIgnoreCase));
        return query.Select(c => (object)new { c.CenterId, c.CenterName, c.OwnerId, c.SubscriptionPlan }).ToList();
    }
}
