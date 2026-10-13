using System.IO;
using System.Xml;

namespace Escandallo.Core;

/// <summary>
/// Inspector XML real con DtdProcessing.Prohibit anti-XXE y escapado físico de fórmulas DDE (SEC-REQ-XLSX-001).
/// </summary>
public static class OoxmlSecurityInspector
{
    private static readonly HashSet<char> FormulaTriggers = new() { '=', '+', '-', '@', '\t', '\r' };

    public static (int HttpStatus, string Verdict) InspectXmlPayloadForXxe(string xmlContent)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };
        try
        {
            using var stringReader = new StringReader(xmlContent);
            using var xmlReader = XmlReader.Create(stringReader, settings);
            while (xmlReader.Read()) { }
            return (200, "XML_SEGURO_SIN_DTD");
        }
        catch (XmlException)
        {
            return (422, "RECHAZADO_DTD_XXE");
        }
    }

    public static string EscapeSpreadsheetFormula(string rawCellText)
    {
        string trimmed = (rawCellText ?? string.Empty).TrimStart();
        return (trimmed.Length > 0 && FormulaTriggers.Contains(trimmed[0])) ? $"'{trimmed}" : rawCellText ?? string.Empty;
    }
}
