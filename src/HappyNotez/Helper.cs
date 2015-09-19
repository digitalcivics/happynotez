using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HappyNotez
{
    internal static class Helper
    {
        internal static void GenerateThumbnail(Stream image, string path)
        {
            BitmapFrame img = BitmapFrame.Create(image);
            double scale = 300.0 / Math.Min(img.PixelHeight, img.PixelWidth);

            TransformedBitmap scaled = new TransformedBitmap(img, new ScaleTransform(scale, scale));

            int startX = (scaled.PixelWidth - 300) / 2;
            int startY = (scaled.PixelHeight - 300) / 2;
            CroppedBitmap cropped = new CroppedBitmap(scaled, new Int32Rect(startX, startY, 300, 300));

            BitmapSource rotated = ApplyOrientation(cropped, img.Metadata as BitmapMetadata);
            using (Stream t = File.Create(path))
            {
                JpegBitmapEncoder jpg = new JpegBitmapEncoder();
                jpg.Frames.Add(BitmapFrame.Create(rotated));
                jpg.Save(t);
            }
        }

        internal static BitmapSource ApplyOrientation(BitmapSource bitmap, BitmapMetadata metadata)
        {
            if (metadata == null || !metadata.ContainsQuery("System.Photo.Orientation"))
                return bitmap;

            ushort orientation = (ushort)metadata.GetQuery("System.Photo.Orientation");

            switch (orientation)
            {
                case 2: // flip horizontal
                    return new TransformedBitmap(bitmap, new ScaleTransform(-1, 1));

                case 3: // rotate 180
                    return new TransformedBitmap(bitmap, new RotateTransform(-180));

                case 4: // flip vertical
                    return new TransformedBitmap(bitmap, new ScaleTransform(1, -1));

                case 5: // transpose
                    bitmap = new TransformedBitmap(bitmap, new ScaleTransform(1, -1));
                    goto case 8;

                case 6: // rotate 270
                    return new TransformedBitmap(bitmap, new RotateTransform(-270));

                case 7: // transverse
                    bitmap = new TransformedBitmap(bitmap, new ScaleTransform(-1, 1));
                    goto case 8;

                case 8: // rotate 90
                    return new TransformedBitmap(bitmap, new RotateTransform(-90));

                default:
                    return bitmap;
            }
        }
    }
}
