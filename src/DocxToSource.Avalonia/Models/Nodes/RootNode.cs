using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DocxToSource.Avalonia.Models.Nodes;

public class RootNode : INode
{
    public INode Parent => null!;
    public string Header => nameof(RootNode);
    public ObservableCollection<INode> Children { get; internal set; } = new();

    public string BuildCodeDomTextDocument(CodeDomProvider provider)
    {
        throw new System.NotImplementedException();
    }

    public string BuildXmlTextDocument()
    {
        throw new System.NotImplementedException();
    }

    public IReadOnlyList<INode> GetSubtreeItems()
    {
        throw new System.NotImplementedException();
    }
}