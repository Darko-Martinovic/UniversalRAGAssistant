using System.Text.Json;
using UniversalRAGAssistant.Models;

namespace UniversalRAGAssistant.Services
{
    public class DocumentLoader
    {
        public async Task<DataConfiguration> LoadDataConfigurationAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Document file not found: {filePath}");
            }

            string jsonContent = await File.ReadAllTextAsync(filePath);

            // Try to deserialize as new format with metadata
            try
            {
                var dataConfig = JsonSerializer.Deserialize<DataConfiguration>(jsonContent);
                if (dataConfig != null && dataConfig.Documents != null)
                {
                    return dataConfig;
                }
            }
            catch (JsonException)
            {
                // If that fails, try to deserialize as old format (just documents array)
                var documents = JsonSerializer.Deserialize<List<DocumentInfo>>(jsonContent);
                if (documents != null)
                {
                    return new DataConfiguration
                    {
                        Metadata = new AppMetadata(), // Use default metadata
                        Documents = documents
                    };
                }
            }

            return new DataConfiguration(); // Return empty configuration if all else fails
        }

        public async Task<List<DocumentInfo>> LoadDocumentsFromJsonAsync(string filePath)
        {
            var dataConfig = await LoadDataConfigurationAsync(filePath);
            return dataConfig.Documents;
        }

        public async Task<List<DocumentInfo>> LoadDocumentsFromTextFilesAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"Documents directory not found: {directoryPath}");
            }

            var documents = new List<DocumentInfo>();
            var textFiles = Directory.GetFiles(directoryPath, "*.txt");

            for (int i = 0; i < textFiles.Length; i++)
            {
                var filePath = textFiles[i];
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var content = await File.ReadAllTextAsync(filePath);

                documents.Add(new DocumentInfo
                {
                    Id = (i + 1).ToString(),
                    Title = fileName.Replace("_", " ").Replace("-", " "), // Clean up filename
                    Content = content.Trim()
                });
            }

            return documents;
        }

        public async Task<List<DocumentInfo>> LoadDocumentsFromCsvAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"CSV file not found: {filePath}");
            }

            var documents = new List<DocumentInfo>();
            var lines = await File.ReadAllLinesAsync(filePath);

            // Skip header row
            for (int i = 1; i < lines.Length; i++)
            {
                var columns = ParseCsvLine(lines[i]);
                if (columns.Length >= 3)
                {
                    documents.Add(new DocumentInfo
                    {
                        Id = columns[0],
                        Title = columns[1],
                        Content = columns[2]
                    });
                }
            }

            return documents;
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = "";
            var inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            result.Add(current.Trim());
            return result.ToArray();
        }
    }
}
