using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Compiler.ViewModels;

public interface IProgramCloser
{
    void CloseProgram();
}

public class MainWindowViewModel : ViewModelBase, IEditorsSet
{
    public ReactiveCommand<Unit, Unit> Create { get; }

    public ReactiveCommand<Unit, Unit> Open { get; }

    public ReactiveCommand<Unit, Unit> Save { get; }

    public ReactiveCommand<Unit, Unit> SaveAs { get; }

    public ReactiveCommand<Unit, Unit> Exit { get; }

    public ReactiveCommand<Unit, Unit> Undo { get; }

    public ReactiveCommand<Unit, Unit> Redo { get; }

    public ReactiveCommand<Unit, Unit> Copy { get; }

    public ReactiveCommand<Unit, Unit> Cut { get; }

    public ReactiveCommand<Unit, Unit> Paste { get; }

    public ReactiveCommand<Unit, Unit> Delete { get; }

    public ReactiveCommand<Unit, Unit> SelectAll { get; }

    public ReactiveCommand<Unit, Unit> CallDocs { get; }

    public ReactiveCommand<Unit, Unit> ShowAboutProgram { get; }

    public ReactiveCommand<Unit, Unit> ZoomIn { get; }
    public ReactiveCommand<Unit, Unit> ZoomOut { get; }

    public ReactiveCommand<Unit, Unit> Run { get; }

    public ReactiveCommand<Unit, Unit> Fix { get; }

    public ReactiveCommand<Unit, Unit> SetTextExample { get; }

    public Interaction<Unit, string?> RequestFilePath { get; } = new();

    private readonly ObservableCollection<EditorViewModel> _editors = [];
    public ReadOnlyObservableCollection<EditorViewModel> Editors => new(_editors);

    [Reactive] public int CurrentEditorIndex { get; set; } = -1;

    [ObservableAsProperty] public EditorViewModel? CurrentEditor { get; }

    public Interaction<Unit, Unit> OpenDocs { get; } = new();
    public Interaction<Unit, Unit> OpenAboutProgram { get; } = new();

    private const int MaxFontSize = 52;
    private const int MinFontSize = 12;
    private const int DefaultFontSize = 16;

    private int _fontSize = DefaultFontSize;

    public int FontSize
    {
        get => _fontSize;
        set =>
            this.RaiseAndSetIfChanged(ref _fontSize,
                value < MinFontSize ? MinFontSize : value > MaxFontSize ? MaxFontSize : value);
    }

    private readonly IFileSaver _fileSaver;

    public MainWindowViewModel(IProgramCloser programCloser, IFileSaver fileSaver)
    {
        _fileSaver = fileSaver;

        this
            .WhenAnyValue(vm => vm.CurrentEditorIndex)
            .Select(i => i >= 0 ? Editors[i] : null)
            .ToPropertyEx(this, vm => vm.CurrentEditor);

        var isFileSelected = this.WhenAnyValue(vm => vm.CurrentEditorIndex, i => i >= 0);

        Create = ReactiveCommand.Create(() => AddEditor(null));
        Open = ReactiveCommand.CreateFromTask(OnOpenEditor);
        Save = ReactiveCommand.CreateFromTask(async () =>
        {
            if (CurrentEditor != null) await CurrentEditor.Save();
        }, isFileSelected);
        SaveAs = ReactiveCommand.CreateFromTask(async () =>
        {
            if (CurrentEditor != null) await CurrentEditor.SaveAs();
        }, isFileSelected);
        Exit = ReactiveCommand.CreateFromTask(async () =>
        {
            await CloseAllEditors();
            programCloser.CloseProgram();
        });

        Undo = ReactiveCommand.Create(() => CurrentEditor?.Edit(EditAction.Undo), isFileSelected);
        Redo = ReactiveCommand.Create(() => CurrentEditor?.Edit(EditAction.Redo), isFileSelected);
        Copy = ReactiveCommand.Create(() => CurrentEditor?.Edit(EditAction.Copy), isFileSelected);
        Cut = ReactiveCommand.Create(() => CurrentEditor?.Edit(EditAction.Cut), isFileSelected);
        Paste = ReactiveCommand.Create(() => CurrentEditor?.Edit(EditAction.Paste), isFileSelected);
        Delete = ReactiveCommand.Create(() => CurrentEditor?.Edit(EditAction.Delete), isFileSelected);
        SelectAll = ReactiveCommand.Create(() => CurrentEditor?.Edit(EditAction.SelectAll), isFileSelected);

        CallDocs = ReactiveCommand.CreateFromTask(async () => { await OpenDocs.Handle(Unit.Default); });
        ShowAboutProgram = ReactiveCommand.CreateFromTask(async () => { await OpenAboutProgram.Handle(Unit.Default); });

        ZoomIn = ReactiveCommand.Create(() => { FontSize++; });
        ZoomOut = ReactiveCommand.Create(() => { FontSize--; });

        Run = ReactiveCommand.Create(() => CurrentEditor?.Run(), isFileSelected);
        Fix = ReactiveCommand.Create(() => CurrentEditor?.Fix(), isFileSelected);
        SetTextExample = ReactiveCommand.Create(() => CurrentEditor?.SetExample(), isFileSelected);
    }

    private async Task OnOpenEditor()
    {
        var filePath = await RequestFilePath.Handle(Unit.Default);
        if (filePath != null) AddEditor(filePath);
    }

    private bool _canEditorBeModified = true;

    private int _untitledEditorsAmount;

    public void AddEditor(string? filePath)
    {
        if (!_canEditorBeModified) return;

        _editors.Add(new EditorViewModel(this, _fileSaver, filePath,
            filePath == null ? ++_untitledEditorsAmount : null));
        CurrentEditorIndex = Editors.Count - 1;
    }

    public void AddEditors(IEnumerable<string> filePaths)
    {
        if (!_canEditorBeModified) return;

        var editors = filePaths
            .Select(filePath => new EditorViewModel(this, _fileSaver, filePath, null));

        _editors.AddRange(editors);
        CurrentEditorIndex = Editors.Count - 1;
    }

    public void RemoveEditor(EditorViewModel editor)
    {
        var nextEditor = NextEditor(editor);

        _editors.Remove(editor);
        CurrentEditorIndex = nextEditor != null ? _editors.IndexOf(nextEditor) : -1;
    }

    private EditorViewModel? NextEditor(EditorViewModel editorToRemove)
    {
        if (CurrentEditor == null)
            return null;

        var editorIndex = Editors.IndexOf(editorToRemove);
        if (editorIndex <= 0)
            return null;

        return editorToRemove.FilePath == CurrentEditor.FilePath ? Editors[editorIndex - 1] : CurrentEditor;
    }

    public async Task CloseAllEditors()
    {
        _canEditorBeModified = false;

        for (var i = 0; i < Editors.Count; i++)
        {
            if (CurrentEditorIndex != i)
            {
                CurrentEditorIndex = i;
                await CurrentEditor!.Activated.Take(1);
            }

            await CurrentEditor!.SaveWithConfirmation();
        }

        _canEditorBeModified = true;
    }
}