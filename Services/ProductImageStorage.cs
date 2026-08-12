using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SmartPOS.Services;

public sealed class ProductImageStorage : IProductImageStorage
{
    private static readonly string Root = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SmartPOS", "ProductImages");

    public async Task<string> CopyFromAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("Không tìm thấy tệp ảnh đã chọn.", sourcePath);
        var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
        if (extension is not ".jpg" and not ".jpeg" and not ".png")
            throw new InvalidOperationException("Chỉ hỗ trợ ảnh JPG hoặc PNG.");

        Directory.CreateDirectory(Root);
        var destination = Path.Combine(Root, $"product_{Guid.NewGuid():N}{extension}");
        await using var source = File.OpenRead(sourcePath);
        await using var target = File.Create(destination);
        await source.CopyToAsync(target, cancellationToken);
        return destination;
    }

    public void DeleteManagedImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return;
        try
        {
            var fullPath = Path.GetFullPath(path);
            if (fullPath.StartsWith(Path.GetFullPath(Root), StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath)) File.Delete(fullPath);
        }
        catch { /* Old image cleanup must never block a successful database update. */ }
    }
}
