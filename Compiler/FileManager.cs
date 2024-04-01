using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using Compiler.ViewModels;

namespace Compiler;

public class FileManager(IStorageProvider storageProvider) : IFileSaver
{
    public async Task AsyncSave(string path, string content)
    {
        try
        {
            await using var outputFile = new StreamWriter(path);
            await outputFile.WriteAsync(content);
            // await TryWriteFile(file, content);
        }
        catch (Exception)
        {
            // TODO
        }
    }

    public async Task<string?> TryRead(string filePath)
    {
        var file = await storageProvider.TryGetFileFromPathAsync(filePath);
        if (file == null) return null;

        try
        {
            return await TryReadFile(file);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static async Task TryWriteFile(IStorageFile file, string content)
    {
        var fileStream = await file.OpenWriteAsync();

        await using var streamWriter = new StreamWriter(fileStream);
        await streamWriter.WriteAsync(content);
        await streamWriter.FlushAsync();
    }

    private static async Task<string> TryReadFile(IStorageFile file)
    {
        var fileStream = await file.OpenReadAsync();

        using var streamWriter = new StreamReader(fileStream);
        var content = await streamWriter.ReadToEndAsync();

        return content;
    }
}