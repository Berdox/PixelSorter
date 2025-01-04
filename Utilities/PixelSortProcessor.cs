using PixelSorter.Models;

namespace PixelSorter.Utilities {
    public static class PixelSortProcessor {
        public static void SortAndApplyWithIntervals(
            PixelImage image,
            Func<PixelImage, IEnumerable<(int Start, int End)>> intervalGenerator,
            Func<IEnumerable<Pixel>, IEnumerable<Pixel>> sorter,
            float randomness) {
            for (int y = 0; y < image.Height; y++) {
                var intervals = intervalGenerator(image);

                foreach (var (start, end) in intervals) {
                    var pixels = new List<Pixel>();

                    for (int x = start; x < end; x++) {
                        var pixel = image.GetPixel(x, y);
                        if (new Random().NextDouble() > randomness) // Randomness check
                        {
                            pixels.Add(pixel);
                        }
                    }

                    var sortedPixels = sorter(pixels);

                    // Apply sorted pixels back to the image
                    int index = 0;
                    for (int x = start; x < end; x++) {
                        if (index < sortedPixels.Count()) {
                            image.SetPixel(sortedPixels.ElementAt(index));
                            index++;
                        }
                    }
                }
            }
        }

        public static void SortAndApplyWithMask(
                    PixelImage image,
                    Func<PixelImage, IEnumerable<(int Start, int End)>> intervalGenerator,
                    Func<IEnumerable<Pixel>, IEnumerable<Pixel>> sorter,
                    float randomness) {
                            for (int y = 0; y < image.Height; y++) {
                                var intervals = intervalGenerator(image);

                                foreach (var (start, end) in intervals) {
                                    var pixels = new List<Pixel>();

                                    for (int x = start; x < end; x++) {
                                        if (image.IsMasked(x, y)) // Check mask
                                        {
                                            var pixel = image.GetPixel(x, y);
                                            if (new Random().NextDouble() > randomness) // Randomness check
                                            {
                                                pixels.Add(pixel);
                                            }
                                        }
                                    }

                    var sortedPixels = sorter(pixels);

                    // Apply sorted pixels back to the image
                    int index = 0;
                    for (int x = start; x < end; x++) {
                        if (index < sortedPixels.Count() && image.IsMasked(x, y)) {
                            image.SetPixel(sortedPixels.ElementAt(index));
                            index++;
                        }
                    }
                }
            }
        }

    }

}
