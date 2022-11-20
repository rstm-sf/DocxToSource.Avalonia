using Microsoft.CSharp;

namespace DocxToSource.Avalonia.Models.Languages;

/// <summary>
/// Definition for the CSharp language.
/// </summary>
public class CSharpLanguageDefinition : LanguageDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CSharpLanguageDefinition"/>
    /// class that is empty.
    /// </summary>
    public CSharpLanguageDefinition() : base(new CSharpCodeProvider(), "C#")
    {
    }
}