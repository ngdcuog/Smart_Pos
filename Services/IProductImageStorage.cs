namespace SmartPOS.Services;

public interface IProductImageStorage
{
    Task<string> CopyFromAsync(string sourcePath, CancellationToken cancellationToken = default);
    void DeleteManagedImage(string? path);
}
