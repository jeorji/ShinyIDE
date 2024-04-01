using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.TextMate;
using Compiler.ViewModels;
using Material.Dialog;
using Material.Dialog.Icons;
using Material.Styles.Themes;
using ReactiveUI;
using TextMateSharp.Grammars;
using CompositeDisposable = System.Reactive.Disposables.CompositeDisposable;
using IDocument = Compiler.ViewModels.IDocument;

namespace Compiler.Views;

public class EditorDocument(ITextSource textSource) : IDocument
{
    public string Text => textSource.Text;

    public IDocument CreateSnapshot()
    {
        return new EditorDocument(textSource.CreateSnapshot());
    }

    public bool Equals(IDocument? other)
    {
        return Text.Equals(other?.Text);
    }
}

internal class EditorDocumentChangedEventArgs(ITextSource newTextSource)
    : IDocumentChangedEventArgs
{
    public IDocument NewDocument { get; } = new EditorDocument(newTextSource);
}

public class Editor : ITextEditor
{
    internal TextEditor TextEditor { get; }

    public IDocument Document => new EditorDocument(TextEditor.Document);

    public event EventHandler<IDocumentChangedEventArgs>? DocumentChanged;

    public Editor(TextEditor textEditor)
    {
        TextEditor = textEditor;
        TextEditor.DocumentChanged += (sender, args) =>
            DocumentChanged?.Invoke(this, new EditorDocumentChangedEventArgs(args.NewDocument));
        TextEditor.Document.Changed += (sender, args) =>
            DocumentChanged?.Invoke(this, new EditorDocumentChangedEventArgs(textEditor.Document.CreateSnapshot()));
    }


    public void UpdateText(string text)
    {
        TextEditor.Document.Text = text;
    }

    public void Undo()
    {
        TextEditor.Undo();
    }

    public void Redo()
    {
        TextEditor.Redo();
    }

    public void Copy()
    {
        TextEditor.Copy();
    }

    public void Cut()
    {
        TextEditor.Cut();
    }

    public void Paste()
    {
        TextEditor.Paste();
    }

    public void Delete()
    {
        TextEditor.Delete();
    }

    public void SelectAll()
    {
        TextEditor.SelectAll();
    }

    public void Select(int start, int end)
    {
        TextEditor.Select(start, end - start);
    }

    public CaretPos OffsetToCaretPos(int offset)
    {
        var location = TextEditor.Document.GetLocation(offset);
        return new CaretPos { Column = location.Column, Row = location.Line };
    }
}

public partial class EditorView : ReactiveUserControl<EditorViewModel>
{
    private readonly FileManager? _fileManager;

    private static Window? GetMainWindow()
    {
        return (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?
            .MainWindow;
    }

    public EditorView()
    {
        InitializeComponent();


        if (GetMainWindow() is { } mainWindow) _fileManager = new FileManager(mainWindow.StorageProvider);

        this.WhenActivated((d) =>
        {
            this.WhenAnyValue(v => v.ViewModel)
                .WhereNotNull()
                .SelectMany(async vm => (vm, vm.TextEditor as Editor ?? await InitEditor()))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Do(PlaceEditorInSlot)
                .Subscribe()
                .DisposeWith(d);

            this.BindInteraction(ViewModel, vm => vm.ConfirmSave, ConfirmSave).DisposeWith(d);
            this.BindInteraction(ViewModel, vm => vm.RequestFilePath, RequestFilePath).DisposeWith(d);
        });
    }

    private CompositeDisposable _textEditorDisposable = new();

    private async Task<Editor> InitEditor()
    {
        var content = await ReadFile(ViewModel?.FilePath) ?? "";

        var textEditor = new TextEditor
        {
            ShowLineNumbers = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Options = new TextEditorOptions
            {
                HighlightCurrentLine = true
            }
        };
        //var theme = Application.Current!.LocateMaterialTheme<MaterialTheme>();
        var selectionColor = Color.FromRgb(1,1,1);
        textEditor.TextArea.SelectionBrush = new SolidColorBrush(selectionColor);

        var registryOptions = new RegistryOptions(ThemeName.Abbys);
        var textMateInstallation = textEditor.InstallTextMate(registryOptions);
        textMateInstallation.SetGrammar(
            registryOptions.GetScopeByLanguageId(registryOptions.GetLanguageByExtension(".rs").Id));

        textEditor.TextArea.Caret.PositionChanged += CaretPosChanged;
        var removePosChangedEvent =
            new CallbackDisposable(() => textEditor.TextArea.Caret.PositionChanged -= CaretPosChanged);

        var binding = ViewModel
            .WhenAnyValue(vm => vm.EditorsSet.FontSize)
            .BindTo(textEditor, te => te.FontSize);

        _textEditorDisposable.Dispose();
        _textEditorDisposable = new CompositeDisposable();

        _textEditorDisposable.Add(removePosChangedEvent);
        binding.DisposeWith(_textEditorDisposable);

        textEditor.Document.BeginUpdate();
        textEditor.Document.Text = content;
        textEditor.Document.EndUpdate();
        textEditor.Document.UndoStack.ClearAll();

        return new Editor(textEditor);
    }

    private class CallbackDisposable(Action action) : IDisposable
    {
        public void Dispose()
        {
            action();
        }
    }


    private void CaretPosChanged(object? sender, EventArgs e)
    {
        if (sender is not Caret caret || ViewModel == null) return;

        ViewModel.CaretPos = new CaretPos
        {
            Row = caret.Line,
            Column = caret.Column
        };
    }

    private void PlaceEditorInSlot((EditorViewModel, Editor) args)
    {
        var (viewModel, editor) = args;
        viewModel.TextEditor ??= editor;

        if (editor.TextEditor.Parent is ContentControl contentControl) contentControl.Content = null;
        TextEditorSlot.Content = editor.TextEditor;
        viewModel.NotifyActive();
    }

    private async Task<string?> ReadFile(string? filePath)
    {
        if (filePath != null && _fileManager != null)
            return await _fileManager.TryRead(filePath);
        return null;
    }

    private async Task ConfirmSave(IInteractionContext<ConfirmSaveParams, ConfirmSaveResult> context)
    {
        var mainWindow = GetMainWindow();
        if (mainWindow == null)
        {
            context.SetOutput(ConfirmSaveResult.Failed);
            return;
        }

        List<DialogButton> buttons = new();

        buttons.Add(
            new DialogButton
            {
                Content = Lang.Resources.SaveDialogDontSave,
                Result = ConfirmSaveResult.DontSave.ToString()
            });

        if (context.Input.Cancellable)
            buttons.Add(new DialogButton
            {
                Content = Lang.Resources.SaveDialogCancel,
                Result = ConfirmSaveResult.Cancel.ToString()
            });

        buttons.Add(
            new DialogButton
            {
                Content = Lang.Resources.SaveDialogSave,
                Result = ConfirmSaveResult.Save.ToString()
            });


        var result = await DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
        {
            WindowTitle = Lang.Resources.WindowWarningTitle,
            ContentHeader = string.Format(Lang.Resources.SaveDialogContentHeader, ViewModel?.FileName),
            SupportingText = Lang.Resources.SaveDialogSupportingText,
            StartupLocation = WindowStartupLocation.CenterOwner,
            DialogHeaderIcon = DialogIconKind.Warning,
            Borderless = true,
            NegativeResult = new DialogResult(ConfirmSaveResult.Cancel.ToString()),
            DialogButtons = buttons.ToArray()
        }).ShowDialog(mainWindow);

        var isParseNotSucceeded = !Enum.TryParse<ConfirmSaveResult>(result.GetResult, out var parsedResult);
        context.SetOutput(isParseNotSucceeded ? ConfirmSaveResult.Failed : parsedResult);
    }

    private async Task RequestFilePath(IInteractionContext<Unit, string?> context)
    {
        if (ViewModel == null || GetMainWindow() is not { } mainWindow)
        {
            context.SetOutput(null);
            return;
        }

        var pickerOptions = await GetSavePickerOptions(ViewModel.FilePath, mainWindow.StorageProvider);
        var file = await mainWindow.StorageProvider.SaveFilePickerAsync(pickerOptions);

        var result = file?.Path.AbsolutePath;
        context.SetOutput(result);
    }

    private static async Task<FilePickerSaveOptions> GetSavePickerOptions(string? prevFilePath,
        IStorageProvider storageProvider)
    {
        var prevDirName = Path.GetDirectoryName(prevFilePath);
        var startLocation = prevDirName != null
            ? await storageProvider.TryGetFolderFromPathAsync(prevDirName)
            : null;

        var prevFile = prevFilePath != null
            ? await storageProvider.TryGetFileFromPathAsync(prevFilePath)
            : null;

        return new FilePickerSaveOptions
        {
            Title = Lang.Resources.SaveFilePickerTitle, SuggestedStartLocation = startLocation,
            SuggestedFileName = prevFile?.Name
        };
    }
}