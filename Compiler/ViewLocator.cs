using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Compiler.ViewModels;
using Compiler.Views;

namespace Compiler;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        return data switch
        {
            EditorViewModel ctx => new EditorView { ViewModel = ctx },
            MainWindowViewModel ctx => new MainWindowView { ViewModel = ctx },
            _ => throw new ArgumentOutOfRangeException(nameof(data), data, null)
        };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}