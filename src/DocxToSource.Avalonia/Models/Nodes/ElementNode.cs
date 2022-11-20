using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using Serialize.OpenXml.CodeGen;

namespace DocxToSource.Avalonia.Models.Nodes;

public class ElementNode : INode
{
    private readonly OpenXmlElement _element;

    public INode Parent { get; }

    public string Header { get; }

    public ObservableCollection<INode> Children { get; }

    public ElementNode(INode parent, OpenXmlElement element, int index)
    {
        _element = element;
        Parent = parent;
        Header = GenerateHeader(element, index);
        Children = new ObservableCollection<INode>(GetSubtreeItems());
    }

    public string BuildCodeDomTextDocument(CodeDomProvider provider) =>
        _element.GenerateSourceCode(provider);

    public string BuildXmlTextDocument()
    {
        var sb = new StringBuilder(_element.OuterXml.Length);

        using var writer = new StringWriter(sb);
        using var xTarget = new XmlTextWriter(writer);

        var xDoc = new XmlDocument();
        xDoc.LoadXml(_element.OuterXml);

        xTarget.Formatting = Formatting.Indented;
        xTarget.Indentation = 2;

        xDoc.Normalize();
        xDoc.PreserveWhitespace = true;
        xDoc.WriteContentTo(xTarget);

        xTarget.Flush();
        xTarget.Close();

        return sb.ToString();
    }

    public IReadOnlyList<INode> GetSubtreeItems() =>
        _element.Elements().GetSubElements(this);

    private static string GenerateHeader(OpenXmlElement element, int index)
    {
        var header = $"<{index}> {element.LocalName} ({element.GetType().Name})";

        switch (element)
        {
            case Row {RowIndex.HasValue: true} row:
                header += $" [{row.RowIndex.Value}]";
                break;
            case Cell {CellReference.HasValue: true} cell:
                header += $" [{cell.CellReference.Value}]";
                break;
        }

        return header;
    }
}