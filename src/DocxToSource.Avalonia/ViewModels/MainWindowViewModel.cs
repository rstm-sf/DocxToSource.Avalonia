using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Highlighting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocxToSource.Avalonia.Models.Languages;
using DocxToSource.Avalonia.Models.Nodes;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using DirectoryInfo = System.IO.DirectoryInfo;
using FileInfo = System.IO.FileInfo;

namespace DocxToSource.Avalonia.ViewModels;

public class MainWindowViewModel : ObservableObject, IDisposable
{
    /// <summary>
    /// Holds the default <see cref="IHighlightingDefinition"/> to use for
    /// the Xml window.
    /// </summary>
    private static readonly IHighlightingDefinition DefaultXmlDefinition = HighlightingManager.Instance.GetDefinition("XML");

    /// <summary>
    /// Holds the <see cref="System.IO.DirectoryInfo"/> object containing the default full path
    /// to use as the initial path in any OpenFileDialog windows.
    /// </summary>
    private static readonly DirectoryInfo DefaultCurrentFileDirectory = new (Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));

    /// <summary>
    /// Holds the source code of the current selected openxml object.
    /// </summary>
    private TextDocument _codeDocument;

    /// <summary>
    /// Holds the highlighting definition for the source code text editor.
    /// </summary>
    private IHighlightingDefinition? _codeSyntax;

    /// <summary>
    /// Holds the <see cref="System.IO.DirectoryInfo"/> object containing the full path
    /// to use as the initial path in any OpenFileDialog windows.
    /// </summary>
    private DirectoryInfo? _currentFileDirectory;

    /// <summary>
    /// Holds the full path and filename of the file that is currently open.
    /// </summary>
    private string? _fileName;

    /// <summary>
    /// Indicates whether or not to automatically generate source code when
    /// selecting DOM nodes.
    /// </summary>
    private bool _generateSourceCode;

    /// <summary>
    /// Indicates whether or not to enable syntax highlighting in the source code
    /// windows.
    /// </summary>
    private bool _highlightSyntax;

    /// <summary>
    /// Indicates whether or not the selected item represents an
    /// <see cref="OpenXmlElement"/> object.
    /// </summary>
    private bool _isOpenXmlElement;

    /// <summary>
    /// Holds the openxml file package the is currently being reviewed.
    /// </summary>
    private OpenXmlPackage? _oPkg;

    /// <summary>
    /// Holds the raw package used to stage the stream information for
    /// validation purposes.
    /// </summary>
    private Package? _pkg;

    /// <summary>
    /// Holds the current treeviewitem that is currently selected in the treeview.
    /// </summary>
    private INode? _selectedItem;

    /// <summary>
    /// Holds the currently selected <see cref="LanguageDefinition"/> object.
    /// </summary>
    private LanguageDefinition _selectedLanguage;

    /// <summary>
    /// Holds the io stream containing the contents of the openxml file package.
    /// </summary>
    private Stream? _stream;

    /// <summary>
    /// Holds the detailed exception information to display in the tree list view.
    /// </summary>
    private readonly RootNode _rootNode;

    /// <summary>
    /// Indicates whether or not to have the text in the source code windows word wrap.
    /// </summary>
    private bool _wordWrap;

    /// <summary>
    /// Holds the xml code fo the current selected openxml element.
    /// </summary>
    private TextDocument _xmlDocument;

    /// <summary>
    /// Holds the highlighting definition for the XML Text Editor
    /// </summary>
    private IHighlightingDefinition? _xmlDocumentSyntax;

    private readonly IDialogService _dialogService;

            /// <summary>
    /// Gets the command to close the current document.
    /// </summary>
    public ICommand CloseCommand { get; private set; }

    /// <summary>
    /// Gets or sets the source code document object to display to the user.
    /// </summary>
    public TextDocument CodeDocument
    {
        get => _codeDocument;
        set
        {
            _codeDocument = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the syntax highlighting definition for the source code text editor.
    /// </summary>
    public IHighlightingDefinition? CodeDocumentSyntax
    {
        get => _codeSyntax;
        set
        {
            _codeSyntax = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the source code text to display to the user.
    /// </summary>
    public string CodeDocumentText
    {
        get => _codeDocument.Text;
        private set
        {
            _codeDocument.Text = value;
            OnPropertyChanged(nameof(CodeDocument));
        }
    }

    /// <summary>
    /// Indicates whether or not to automatically generate source code when
    /// selecting DOM nodes.
    /// </summary>
    public bool GenerateSourceCode
    {
        get => _generateSourceCode;
        set
        {
            _generateSourceCode = value;
            var item = GenerateSourceCode ? SelectedItem : null;
            RefreshSourceCodeWindows(item);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether or not to enable syntax highlighting in the source code
    /// windows.
    /// </summary>
    public bool HighlightSyntax
    {
        get => _highlightSyntax;
        set
        {
            _highlightSyntax = value;
            ToggleSyntaxHighlighting(GenerateSourceCode && !(SelectedItem is null) && _highlightSyntax);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether or not the selected item represents an
    /// <see cref="OpenXmlElement"/> object.
    /// </summary>
    public bool IsOpenXmlElement
    {
        get => _isOpenXmlElement;
        set
        {
            _isOpenXmlElement = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection of <see cref="LanguageDefinition"/> objects that
    /// the user can select.
    /// </summary>
    public ReadOnlyObservableCollection<LanguageDefinition> LanguageDefinitions
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets the command to open a new office 2007+ document.
    /// </summary>
    public ICommand OpenCommand { get; private set; }

    /// <summary>
    /// Gets the command that shuts down the application.
    /// </summary>
    public ICommand QuitCommand { get; private set; }

    /// <summary>
    /// Gets or sets the <see cref="INode"/> that is currently selected.
    /// </summary>
    public INode? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;

            if (GenerateSourceCode)
                RefreshSourceCodeWindows(_selectedItem);

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="LanguageDefinition"/> object currently
    /// selected by the user.
    /// </summary>
    public LanguageDefinition SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            _selectedLanguage = value;
            if (GenerateSourceCode)
            {
                RefreshSourceCodeWindows(SelectedItem);
            }
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets all of the openxml objects to display in the tree.
    /// </summary>
    public ObservableCollection<INode> TreeData
    {
        get => _rootNode.Children;
        private set
        {
            _rootNode.Children = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Indicates whether or not to have the text in the source code windows word wrap.
    /// </summary>
    public bool WordWrap
    {
        get => _wordWrap;
        set
        {
            _wordWrap = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the source code to display to the user.
    /// </summary>
    public TextDocument XmlSourceDocument
    {
        get => _xmlDocument;
        set
        {
            _xmlDocument = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the syntax highlighting definition for the xml document text editor.
    /// </summary>
    public IHighlightingDefinition? XmlSourceDocumentSyntax
    {
        get => _xmlDocumentSyntax;
        set
        {
            _xmlDocumentSyntax = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the source code text to display to the user.
    /// </summary>
    public string XmlSourceDocumentText
    {
        get => _xmlDocument.Text;
        set
        {
            _xmlDocument.Text = value;
            OnPropertyChanged(nameof(XmlSourceDocument));
        }
    }

    /// <remarks>
    /// Just for previewer
    /// </remarks>
    internal MainWindowViewModel() : this(null!)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class that
    /// is empty.
    /// </summary>
    public MainWindowViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;

        _currentFileDirectory = DefaultCurrentFileDirectory;

        _rootNode = new RootNode();

        var langeDefs = new ObservableCollection<LanguageDefinition>();
        LanguageDefinitions = new ReadOnlyObservableCollection<LanguageDefinition>(langeDefs);

        // Load the language definition list
        langeDefs.Add(new CSharpLanguageDefinition());
        langeDefs.Add(new VBLanguageDefinition());

        _selectedLanguage = LanguageDefinitions[0];

        _codeDocument = new TextDocument();
        _xmlDocument = new TextDocument();

        CloseCommand = new RelayCommand(() =>
        {
            Dispose();
            TreeData.Clear();
            RefreshSourceCodeWindows(null);
        });
        OpenCommand = new AsyncRelayCommand(OpenOfficeDocument);

        QuitCommand = new RelayCommand(() =>
        {
            Dispose();
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                desktop.Shutdown();
        });
    }

    /// <summary>
    /// Method to make sure that all unmanaged resources are released properly.
    /// </summary>
    public void Dispose()
    {
        if (_oPkg != null)
        {
            _oPkg.Close();
            _oPkg.Dispose();
            _oPkg = null;
        }
        if (_pkg != null)
        {
            _pkg.Close();
            _pkg = null;
        }
        if (_stream != null)
        {
            _stream.Close();
            _stream.Dispose();
            _stream = null;
        }
    }

    /// <summary>
    /// Resets the main window controls and loads a requested OpenXml based file.
    /// </summary>
    private async Task OpenOfficeDocument()
    {
        var filepath = await _dialogService.ShowOpenFileDialogAsync(this, settings: Settings());
        if (filepath == null) return;

        const string docxIdUri = "/word/document.xml";
        const string xlsxIdUri = "/xl/workbook.xml";
        const string pptxIdUri = "/ppt/presentation.xml";

        // Ensure that everything is cleared out before proceeding
        Dispose();
        CodeDocument.FileName = null;
        XmlSourceDocument.FileName = null;
        CodeDocumentText = string.Empty;
        XmlSourceDocumentText = string.Empty;
        _fileName = string.Empty;

        // Get the selected file details
        var fi = new FileInfo(filepath);
        _currentFileDirectory = fi.Directory;
        _fileName = fi.Name;
        var buffer = await File.ReadAllBytesAsync(filepath);
        _stream = new MemoryStream();
        await _stream.WriteAsync(buffer);
        _pkg = Package.Open(_stream);

        // Setup a quick look up for easier package validation
        var quickPicks = new Dictionary<string, Func<Package, OpenXmlPackage>>(3)
        {
            { docxIdUri, WordprocessingDocument.Open },
            { xlsxIdUri, SpreadsheetDocument.Open },
            { pptxIdUri, PresentationDocument.Open }
        };

        foreach (var qp in quickPicks.Where(qp => _pkg.PartExists(new Uri(qp.Key, UriKind.Relative))))
        {
            _oPkg = qp.Value.Invoke(_pkg);
            break;
        }

        // Make sure that a valid package was found before proceeding.
        if (_oPkg == null)
        {
            throw new InvalidDataException("Selected file is not a known/valid OpenXml document");
        }

        // Wrap it up
        TreeData.Clear();
        TreeData.Add(new PackageNode(_rootNode, _oPkg, _fileName));
    }

    private OpenFileDialogSettings Settings() =>
        new()
        {
            InitialDirectory = _currentFileDirectory?.FullName ?? DefaultCurrentFileDirectory.FullName
        };

    /// <summary>
    /// Refreshes the <see cref="TextEditor"/> controls
    /// in the main window.
    /// </summary>
    /// <param name="item">
    /// The <see cref="INode"/> currently selected by the user.
    /// </param>
    /// <remarks>
    /// Passing <see langword="null"/> as the <paramref name="item"/> will cause
    /// the <see cref="TextEditor"/> controls to clear their
    /// contents.
    /// </remarks>
    private void RefreshSourceCodeWindows(INode? item)
    {
        if (item is null)
        {
            CodeDocument.FileName = null;
            XmlSourceDocument.FileName = null;
            CodeDocumentText = string.Empty;
            XmlSourceDocumentText = string.Empty;
            ToggleSyntaxHighlighting(false);
        }
        else
        {
            var randName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
            CodeDocument.FileName = randName + "." + SelectedLanguage.Provider.FileExtension;
            XmlSourceDocument.FileName = randName + ".xml";

            CodeDocumentText = item.BuildCodeDomTextDocument(SelectedLanguage.Provider);
            XmlSourceDocumentText = item.BuildXmlTextDocument();

            ToggleSyntaxHighlighting(HighlightSyntax);
        }
        IsOpenXmlElement = !string.IsNullOrWhiteSpace(XmlSourceDocumentText);
    }

    /// <summary>
    /// Enables or disables syntax highlighting in the code windows.
    /// </summary>
    /// <param name="enable">
    /// <see langword="true"/> to turn on syntax highlighting; <see langword="false"/>
    /// to turn it off.
    /// </param>
    private void ToggleSyntaxHighlighting(bool enable)
    {
        CodeDocumentSyntax = enable ? SelectedLanguage.Highlighting : null;
        XmlSourceDocumentSyntax = enable ? DefaultXmlDefinition : null;
    }
}
