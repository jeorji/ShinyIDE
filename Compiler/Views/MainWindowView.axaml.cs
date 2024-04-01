using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.ReactiveUI;
using Compiler.ViewModels;
using ReactiveUI;

namespace Compiler.Views;

public partial class MainWindowView : ReactiveWindow<MainWindowViewModel>, IProgramCloser
{
    public MainWindowView()
    {
        InitializeComponent();

        Closing += HandleClosing;

        this.WhenActivated(d =>
        {
            this
                .BindInteraction(ViewModel, vm => vm.RequestFilePath, RequestFilePath)
                .DisposeWith(d);

            this
                .BindInteraction(ViewModel, vm => vm.OpenDocs, async context =>
                {
                    var docsWindow = new DocsWindowView();
                    docsWindow.DataContext = docsWindow;
                    await docsWindow.ShowDialog(this);
                    context.SetOutput(Unit.Default);
                })
                .DisposeWith(d);

            this
                .BindInteraction(ViewModel, vm => vm.OpenAboutProgram, async context =>
                {
                    //await ShowDialog(new DocsWindowView());
                    context.SetOutput(Unit.Default);
                })
                .DisposeWith(d);
        });
    }

    private async void HandleClosing(object? sender, WindowClosingEventArgs e)
    {
        e.Cancel = true;
        if (ViewModel != null) await ViewModel.CloseAllEditors();

        Closing -= HandleClosing;
        Close();
    }

    private async Task RequestFilePath(IInteractionContext<Unit, string?> context)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { AllowMultiple = false });

        if (files.Count == 0)
        {
            context.SetOutput(null);
            return;
        }

        context.SetOutput(files[0].Path.AbsolutePath);
    }

    public void CloseProgram()
    {
        Close();
    }
}