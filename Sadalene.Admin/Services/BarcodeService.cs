using QRCoder;

namespace Sadalene.Admin.Services;

public class BarcodeService
{
    private readonly IWebHostEnvironment _env;

    public BarcodeService(IWebHostEnvironment env) => _env = env;

    public string GenerateQrCode(string value, string fileName)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(10);

        var folder = Path.Combine(_env.WebRootPath, "uploads", "barcodes");
        Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, $"{fileName}.png");
        File.WriteAllBytes(filePath, pngBytes);

        return $"/uploads/barcodes/{fileName}.png";
    }
}
