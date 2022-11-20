using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DocxToSource.Avalonia.Models.Nodes;

public interface INode
{
    INode Parent { get; }
    string Header { get; }
    ObservableCollection<INode> Children { get; }

    string BuildCodeDomTextDocument(CodeDomProvider provider);
    string BuildXmlTextDocument();

    IReadOnlyList<INode> GetSubtreeItems();
}
