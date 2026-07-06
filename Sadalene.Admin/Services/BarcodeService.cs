using QRCoder;

namespace Sadalene.Admin.Services;

public class BarcodeService
{
    public byte[] GenerateQrCode(string value)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrData);
        return qrCode.GetGraphic(10);
    }
}
