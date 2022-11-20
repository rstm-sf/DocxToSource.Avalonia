using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Serialize.OpenXml.CodeGen;

namespace DocxToSource.Avalonia.Models.Nodes;

public class PartNode : INode
{
    private readonly IdPartPair _part;

    public INode Parent { get; }

    public string Header { get; }

    public ObservableCollection<INode> Children { get; }

    public PartNode(INode parent, IdPartPair part)
    {
        _part = part;
        Parent = parent;
        Header = $"[{part.RelationshipId}] {part.OpenXmlPart.Uri} ({part.OpenXmlPart.GetType().Name})";
        Children = new ObservableCollection<INode>(GetSubtreeItems());
    }

    public string BuildCodeDomTextDocument(CodeDomProvider provider) =>
        _part.OpenXmlPart.GenerateSourceCode(provider);

    public string BuildXmlTextDocument() => "";

    public IReadOnlyList<INode> GetSubtreeItems()
    {
        var result = _part.OpenXmlPart.Parts.SelectSubParts(this);
        if (_part.OpenXmlPart.RootElement?.Any() != true) return result;

        var elements = _part.OpenXmlPart.RootElement.GetSubElements(this);
        result.AddRange(elements);

        return result;
    }
}