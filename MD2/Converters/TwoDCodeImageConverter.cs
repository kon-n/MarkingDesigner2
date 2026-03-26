using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MarkingDesigner.Models;
using MarkingDesigner.ViewModels;
using ZXing;
using ZXing.Common;

namespace MarkingDesigner.Converters
{
    /// <summary>
    /// SequenceJobEntry から 2D コード画像を生成する。
    /// </summary>
    public sealed class TwoDCodeImageConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not SequenceJobEntry entry || !entry.IsTwoDCommand)
                return null;

            var payload = string.Join("", entry.TwoDSourceJobs.Select(j => j.Marking.Text ?? ""));
            if (string.IsNullOrWhiteSpace(payload))
                payload = entry.Job?.Marking.Text ?? "EMPTY";

            var format = entry.Role switch
            {
                SequenceJobRole.DataMatrix => BarcodeFormat.DATA_MATRIX,
                SequenceJobRole.QrCode => BarcodeFormat.QR_CODE,
                SequenceJobRole.MicroQr => BarcodeFormat.QR_CODE, // microQR は環境依存のため QR として生成
                _ => BarcodeFormat.QR_CODE
            };

            // 表示寸法は XAML 側で SymbolSizeV(mm) に合わせる。
            // ここでは読み取りやすさを優先して内部画像を高解像度で生成する。
            const int pixelsPerMm = 24;
            int size = Math.Clamp(entry.SymbolSizeV * pixelsPerMm, 192, 2048);

            var writer = new BarcodeWriterPixelData
            {
                Format = format,
                Options = new EncodingOptions
                {
                    Width = size,
                    Height = size,
                    Margin = 1,
                    PureBarcode = true
                }
            };

            var pixelData = writer.Write(payload);

            return BitmapSource.Create(
                pixelData.Width,
                pixelData.Height,
                96,
                96,
                System.Windows.Media.PixelFormats.Bgra32,
                null,
                pixelData.Pixels,
                pixelData.Width * 4);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
