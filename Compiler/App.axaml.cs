using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Compiler.ViewModels;
using Compiler.Views;

namespace Compiler;

public partial class App : Application
{
    public override void Initialize()
    {
        // Assets.Resources.Culture = new CultureInfo("en-EN");
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindowView();
            mainWindow.ViewModel = new MainWindowViewModel(mainWindow, new FileManager(mainWindow.StorageProvider));
            desktop.MainWindow = mainWindow;
        }


        base.OnFrameworkInitializationCompleted();
    }
}