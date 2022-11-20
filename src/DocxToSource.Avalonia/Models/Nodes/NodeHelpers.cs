using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;

namespace DocxToSource.Avalonia.Models.Nodes;

internal static class NodeHelpers
{
    public static List<INode> SelectSubParts(this IEnumerable<IdPartPair> parts, INode parent) =>
        parts.Select(subpart => new PartNode(parent, subpart)).ToList<INode>();

    public static List<INode> GetSubElements(this IEnumerable<OpenXmlElement> elements, INode parent) =>
        elements.Select((element, index) => new ElementNode(parent, element, index)).ToList<INode>();
}