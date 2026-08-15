using ClinicManagementSystem.Application.Common.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace ClinicManagementSystem.Infrastructure.ExternalServices;

public sealed class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(
        IWebHostEnvironment environment,
        ILogger<FileStorageService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string folderName, 
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        string webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        string targetDirectory = Path.Combine(webRootPath, folderName);

        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        string fileExtension = Path.GetExtension(fileName);
        string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        string fullPath = Path.Combine(targetDirectory, uniqueFileName);

        await using (var destinationStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
        {
            await fileStream.CopyToAsync(destinationStream, ct);
        }

        _logger.LogInformation("File successfully uploaded: {FileName} to path: {FolderPath}", uniqueFileName, folderName);

        string relativeUrl = $"/{folderName.Trim('/', '\\')}/{uniqueFileName}";
        return relativeUrl.Replace('\\', '/');
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return Task.CompletedTask;

        try
        {
            string webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string cleanRelativePath = fileUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
            string fullPath = Path.Combine(webRootPath, cleanRelativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("File deleted successfully from path: {FilePath}", fullPath);
            }
            else
            {
                _logger.LogWarning("File deletion skipped. File not found at path: {FilePath}", fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while attempting to delete file: {FileUrl}", fileUrl);
        }

        return Task.CompletedTask;
    }
}