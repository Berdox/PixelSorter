
namespace PixelSorter.Utilities {

    public static class IntervalGenerators {
        // Edge-based intervals
        public static IEnumerable<(int Start, int End)> GenerateEdgeIntervals(PixelImage image, float lowerThreshold) {
            var edgeImage = image.GetEdgeImage();
            for (int y = 0; y < image.Height; y++) {
                int start = -1;
                for (int x = 0; x < image.Width; x++) {
                    var pixel = edgeImage.GetPixel(x, y);
                    if (pixel.Color.Brightness > lowerThreshold * 255) {
                        if (start == -1) start = x;
                    }
                    else if (start != -1) {
                        yield return (start, x);
                        start = -1;
                    }
                }
            }
        }

        // Threshold-based intervals
        public static IEnumerable<(int Start, int End)> GenerateThresholdIntervals(PixelImage image, float lowerThreshold, float upperThreshold) {
            for (int y = 0; y < image.Height; y++) {
                int start = -1;
                for (int x = 0; x < image.Width; x++) {
                    var pixel = image.GetPixel(x, y);
                    if (pixel.Color.Brightness >= lowerThreshold * 255 && pixel.Color.Brightness <= upperThreshold * 255) {
                        if (start == -1) start = x;
                    }
                    else if (start != -1) {
                        yield return (start, x);
                        start = -1;
                    }
                }
            }
        }

        // Random intervals
        public static IEnumerable<(int Start, int End)> GenerateRandomIntervals(PixelImage image, int averageLength) {
            Random rand = new();
            for (int y = 0; y < image.Height; y++) {
                int start = 0;
                while (start < image.Width) {
                    int length = rand.Next(averageLength / 2, averageLength * 2);
                    int end = Math.Min(start + length, image.Width);
                    yield return (start, end);
                    start = end;
                }
            }
        }

        // Wave-based intervals
        public static IEnumerable<(int Start, int End)> GenerateWaveIntervals(PixelImage image, int averageLength, int waveVariance) {
            Random rand = new();
            for (int y = 0; y < image.Height; y++) {
                int start = 0;
                while (start < image.Width) {
                    int length = averageLength + rand.Next(-waveVariance, waveVariance);
                    int end = Math.Min(start + Math.Max(1, length), image.Width);
                    yield return (start, end);
                    start = end;
                }
            }
        }
    }
}
