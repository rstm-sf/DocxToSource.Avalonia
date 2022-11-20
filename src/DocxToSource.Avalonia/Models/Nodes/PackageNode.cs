using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using Serialize.OpenXml.CodeGen;

namespace DocxToSource.Avalonia.Models.Nodes;

public class PackageNode : INode
{
    private readonly OpenXmlPackage _package;

    public INode Parent { get; }

    public string Header { get; }

    public ObservableCollection<INode> Children { get; }

    public PackageNode(INode parent, OpenXmlPackage package, string header)
    {
        _package = package;
        Parent = parent;
        Header = header;
        Children = new ObservableCollection<INode>(GetSubtreeItems());
    }

    public string BuildCodeDomTextDocument(CodeDomProvider provider) =>
        _package.GenerateSourceCode(provider);

    public string BuildXmlTextDocument() => "";

    public IReadOnlyList<INode> GetSubtreeItems() =>
        _package.Parts.SelectSubParts(this);
}