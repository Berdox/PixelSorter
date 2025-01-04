using PixelSorter.Models;

namespace PixelSorter.Utilities {

    public static class PatternExtractors {
        public static IEnumerable<Pixel> GetRow(PixelImage image, int row) {
            for (int x = 0; x < image.Width; x++)
                yield return image.GetPixel(x, row);
        }

        public static IEnumerable<Pixel> GetColumn(PixelImage image, int column) {
            for (int y = 0; y < image.Height; y++)
                yield return image.GetPixel(column, y);
        }

        public static IEnumerable<Pixel> GetDiagonal(PixelImage image, int startX, int startY) {
            int x = startX, y = startY;
            while (x < image.Width && y < image.Height) {
                yield return image.GetPixel(x, y);
                x++;
                y++;
            }
        }

        public static IEnumerable<Pixel> GetCircularPattern(PixelImage image, int centerX, int centerY, int radius) {
            for (int angle = 0; angle < 360; angle++) {
                int x = (int)(centerX + radius * Math.Cos(angle * Math.PI / 180));
                int y = (int)(centerY + radius * Math.Sin(angle * Math.PI / 180));

                if (x >= 0 && y >= 0 && x < image.Width && y < image.Height)
                    yield return image.GetPixel(x, y);
            }
        }

        public static IEnumerable<Pixel> GetCustomPattern(PixelImage image, Func<int, int, bool> predicate) {
            for (int y = 0; y < image.Height; y++) {
                for (int x = 0; x < image.Width; x++) {
                    if (predicate(x, y))
                        yield return image.GetPixel(x, y);
                }
            }
        }
    }

}
