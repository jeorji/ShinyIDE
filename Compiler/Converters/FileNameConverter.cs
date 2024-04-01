using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Compiler.Converters;

public class FileNameConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 2) return BindingNotification.UnsetValue;

        var fileName = values[0];
        var untitledFileIndex = values[1];

        return fileName switch
        {
            string => fileName,
            null when untitledFileIndex is int => string.Format(Lang.Resources.UntitledFileName, untitledFileIndex),
            _ => ""
        };
    }
}