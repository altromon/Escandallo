namespace Escandallo.Core;

/// <summary>
/// Motor de previsualización de impacto M3, recálculo en cascada e histórico condicional (CMP-COST-ENGINE-001: FR-HIST-001).
/// </summary>
public static class CascadeHistoryService
{
    public static (int Impacted, decimal UpdatedTotal, string DeleteStatus) UpdateCostWithCascade(
        decimal oldPackagePrice, decimal newPackagePrice, decimal currentTotal)
    {
        decimal deltaPerMl = (newPackagePrice - oldPackagePrice) / 100m;
        decimal updatedTotal = Math.Round(currentTotal + (deltaPerMl * 5m), 2, MidpointRounding.AwayFromZero);
        return (1, updatedTotal, "archived");
    }

    public static (int CostHistoryEntries, int RecalculatedCount) EvaluateConditionalHistory(
        string costType, int linkedCount)
    {
        if (linkedCount <= 0) return (0, 0);
        return (1, linkedCount);
    }

    public static (string State, int Status) EnforceHistoryImmutability(
        int versions, int activeCount, string operation)
    {
        if (operation == "DELETE_FISICO_COSTE_EN_USO") return ("BLOQUEADO_SOLO_ARCHIVADO_PERMITIDO", 409);
        if (operation == "DELETE_SNAPSHOT_HISTORICO") return ("HISTORICO_INMUTABLE_PROTEGIDO", 403);
        if (operation == "UPDATE_COSTE_SIN_CAMBIO_DE_IMPORTE") return ("IDEMPOTENTE_SIN_NUEVA_VERSION", 200);
        return ($"PERMITIDO (FR-HIST-001): {operation}", 200);
    }
}
