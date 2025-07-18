using System;
using System.IO;
using System.Linq;

namespace Abstractions.Services;

public class XmlLogService : IXmlLogService
{
    private readonly string _fileName = GenerateNextFileName();

    private static string GenerateNextFileName()
    {
        var nextNumber = 1;
        const string basePattern = "ResponseXml_*.xml";

            var existingFiles = Directory.GetFiles(Environment.CurrentDirectory, basePattern);
                
            if (existingFiles.Length != 0)
            {
                nextNumber = existingFiles
                    .Select(Path.GetFileNameWithoutExtension)
                    .Where(file => file != null && file.StartsWith("ResponseXml_"))
                    .Select(file =>
                    {
                        var numberPart = file?["ResponseXml_".Length..];
                        return int.TryParse(numberPart, out var num) ? num : 0;
                    })
                    .Max() + 1;
            }
        return Path.Combine(Environment.CurrentDirectory, $"ResponseXml_{nextNumber}.xml");
    }

    public void LogResponse(string response)
    {
        if (!File.Exists(_fileName))
        {
            File.WriteAllText(_fileName, "");
        }

        try
        {
            File.AppendAllText(_fileName, $"{response} \n\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write to file: {ex}");
        }
    }
}