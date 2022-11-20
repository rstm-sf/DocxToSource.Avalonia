using Microsoft.VisualBasic;

namespace DocxToSource.Avalonia.Models.Languages;

/// <summary>
/// Definition for the Visual Basic.net language.
/// </summary>
public class VBLanguageDefinition : LanguageDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VBLanguageDefinition"/>
    /// class that is empty.
    /// </summary>
    public VBLanguageDefinition() : base(new VBCodeProvider(), "Visual Basic.Net")
    {
    }
}