using System;
using System.CodeDom.Compiler;
using AvaloniaEdit.Highlighting;
using Serialize.OpenXml.CodeGen;

namespace DocxToSource.Avalonia.Models.Languages;

/// <summary>
/// Base class for language definitions for the project.
/// </summary>
public abstract class LanguageDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageDefinition"/> class
    /// with the <see cref="CodeDomProvider"/> object that will be used to
    /// create the language source code.
    /// </summary>
    /// <param name="provider">
    /// The <see cref="CodeDomProvider"/> object for the new <see cref="LanguageDefinition"/>
    /// provider.
    /// </param>
    /// <param name="displayName">The name to display to the user</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="provider"/> is <see langword="null"/>.
    /// </exception>
    protected LanguageDefinition(CodeDomProvider provider, string displayName)
        : this(NamespaceAliasOptions.Default, provider, displayName) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LanguageDefinition"/> class
    /// with the <see cref="CodeDomProvider"/> object that will be used to
    /// create the language source code and predefined <see cref="NamespaceAliasOptions"/>.
    /// </summary>
    /// <param name="opts">
    /// Custom <see cref="NamespaceAliasOptions"/> for the language definition.
    /// </param>
    /// <param name="provider">
    /// The <see cref="CodeDomProvider"/> object for the new <see cref="LanguageDefinition"/>
    /// provider.
    /// </param>
    /// <param name="displayName">The name to display to the user</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="opts"/> or <paramref name="provider"/> is <see langword="null"/>.
    /// </exception>
    protected LanguageDefinition(NamespaceAliasOptions opts, CodeDomProvider provider, string displayName)
    {
        Options = opts ?? throw new ArgumentNullException(nameof(opts));
        Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        DisplayName = displayName;
        Highlighting = HighlightingManager.Instance.GetDefinitionByExtension("." + Provider.FileExtension);
    }

    /// <summary>
    /// Gets the name to display to the user.
    /// </summary>
    public string DisplayName { get; protected set; }

    /// <summary>
    /// Gets the <see cref="IHighlightingDefinition"/> to use for the generated
    /// source code.
    /// </summary>
    public IHighlightingDefinition Highlighting { get; private set; }

    /// <summary>
    /// Gets the <see cref="NamespaceAliasOptions"/> to use when generating
    /// source code.
    /// </summary>
    public NamespaceAliasOptions Options { get; private set; }

    /// <summary>
    /// Gets the <see cref="CodeDomProvider"/> to use when generating
    /// source code.
    /// </summary>
    public CodeDomProvider Provider { get; private set; }

    /// <inheritdoc/>
    public override string ToString() => DisplayName;
}