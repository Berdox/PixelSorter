using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace PixelSorter.src
{
    public enum SortingType
    {
        BRIGHTNESS,
        HUE,
        SATURATION,
        GRAYSCALE,
        ALPHA,
        RANDOM,
        WAVE,
        COLOR_FREQUENCY
    }

    public enum SortingAxis
    {
        COLUMN,
        ROW,
        DIAGONAL
    }

    public enum SortingDirection
    {
        LEFT,
        RIGHT
    }

    public enum StartingPoint
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public static class Sorter
    {

        // 1. Get image
        // 2. are you sorting by row, col, or diaginaol.
        // 3. Then also left or right for col and row. For diaginal starting position top left, top right, bottom right, or bottom left.
        // 4. what type of sorting are you doing (brightness, hue, saturation, Luminance, Alpha, grayscale, Graident, random)
        // 5. Ex. Luminace are we going from brightest to dimest or vice versa

        private static Random _random = new Random();

        public static void SortByRow(string inputPath, string outputPath, SortingType type, SortingDirection direction)
        {

            Image<Rgba32> image;

            try
            {
                image = ReadImage(inputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load image: {ex.Message}");
                return;
            }

            for (int y = 0; y < image.Height; y++)
            {

                var rowPixels = new List<Rgba32>();
                for (int x = 0; x < image.Width; x++)
                {
                    rowPixels.Add(image[x, y]);
                }

                var sortedPixels = SortPixels(rowPixels, type, direction, y);

                for (int x = 0; x < image.Width; x++)
                {
                    image[x, y] = sortedPixels[x];
                }
            }

            image.Save(outputPath);
            Console.WriteLine("Image saved to " + outputPath);

        }

        public static void SortByColumn(string inputPath, string outputPath, SortingType type, SortingDirection direction)
        {

            Image<Rgba32> image;

            try
            {
                image = ReadImage(inputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load image: {ex.Message}");
                return;
            }

            for (int x = 0; x < image.Width; x++)
            {

                var rowPixels = new List<Rgba32>();
                for (int y = 0; y < image.Height; y++)
                {
                    rowPixels.Add(image[x, y]);
                }

                var sortedPixels = SortPixels(rowPixels, type, direction, x);

                for (int y = 0; y < image.Height; y++)
                {
                    image[x, y] = sortedPixels[y];
                }
            }

            image.Save(outputPath);
            Console.WriteLine("Image saved to " + outputPath);
        }

        public static void SortByDiagonal(string inputPath, string outputPath, SortingType type, SortingDirection direction, StartingPoint startingPoint)
        {
            Image<Rgba32> image;

            try
            {
                image = ReadImage(inputPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load image: {ex.Message}");
                return;
            }

            int width = image.Width;
            int height = image.Height;

            // Iterate over all diagonals based on the starting point
            for (int d = 0; d < width + height - 1; d++)
            {
                var diagonalPixels = new List<Rgba32>();

                // Collect pixels based on the starting point
                if (startingPoint == StartingPoint.TopLeft)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int y = d - x;
                        if (y >= 0 && y < height)
                        {
                            diagonalPixels.Add(image[x, y]);
                        }
                    }
                }
                else if (startingPoint == StartingPoint.TopRight)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int y = d - (width - 1 - x);
                        if (y >= 0 && y < height)
                        {
                            diagonalPixels.Add(image[x, y]);
                        }
                    }
                }
                else if (startingPoint == StartingPoint.BottomLeft)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int y = height - 1 - d + x;
                        if (y >= 0 && y < height)
                        {
                            diagonalPixels.Add(image[x, y]);
                        }
                    }
                }
                else if (startingPoint == StartingPoint.BottomRight)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int y = height - 1 - d + (width - 1 - x);
                        if (y >= 0 && y < height)
                        {
                            diagonalPixels.Add(image[x, y]);
                        }
                    }
                }

                // Sort the pixels
                var sortedPixels = SortPixels(diagonalPixels, type, direction);

                // Place the sorted pixels back into the diagonal
                int index = 0;
                if (startingPoint == StartingPoint.TopLeft || startingPoint == StartingPoint.TopRight)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int y = startingPoint == StartingPoint.TopLeft ? d - x : d - (width - 1 - x);
                        if (y >= 0 && y < height)
                        {
                            image[x, y] = sortedPixels[index++];
                        }
                    }
                }
                else if (startingPoint == StartingPoint.BottomLeft || startingPoint == StartingPoint.BottomRight)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int y = startingPoint == StartingPoint.BottomLeft ? height - 1 - d + x : height - 1 - d + (width - 1 - x);
                        if (y >= 0 && y < height)
                        {
                            image[x, y] = sortedPixels[index++];
                        }
                    }
                }
            }

            image.Save(outputPath);
            Console.WriteLine("Image saved to " + outputPath);
        }

        public static void SortWithMask(string inputPath, string outputPath, string maskPath, SortingType type, SortingDirection direction)
        {
            Image<Rgba32> image, mask;

            try
            {
                // Load the target image and mask
                image = ReadImage(inputPath);
                mask = ReadImage(maskPath);

                if (image.Width != mask.Width || image.Height != mask.Height)
                {
                    throw new InvalidOperationException("The mask dimensions must match the input image dimensions.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load image or mask: {ex.Message}");
                return;
            }

            for (int y = 0; y < image.Height; y++)
            {
                var pixelsToSort = new List<Rgba32>();
                var indices = new List<int>();

                for (int x = 0; x < image.Width; x++)
                {
                    // Check the mask value (e.g., use the brightness of the mask pixel)
                    var maskPixel = mask[x, y];
                    float maskBrightness = maskPixel.SortByBrightness();

                    // Include pixels where the mask is "active" (e.g., brightness > 0.5)
                    if (maskBrightness < 0.5f)
                    {
                        pixelsToSort.Add(image[x, y]);
                        indices.Add(x);
                    }
                }

                // Sort the selected pixels
                var sortedPixels = SortPixels(pixelsToSort, type, direction, y);

                // Place the sorted pixels back in their positions
                for (int i = 0; i < indices.Count; i++)
                {
                    image[indices[i], y] = sortedPixels[i];
                }
            }

            // Save the modified image
            image.Save(outputPath);
            Console.WriteLine("Image saved to " + outputPath);
        }


        private static Image<Rgba32> ReadImage(string inputPath)
        {
            Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());

            // Check if file exists
            if (!File.Exists(inputPath))
            {
                throw new FileNotFoundException($"The file at {inputPath} does not exist.");
            }

            try
            {
                return Image.Load<Rgba32>(inputPath);
            }
            catch (ImageProcessingException ex)
            {
                throw new InvalidOperationException($"Error processing the image: {ex.Message}", ex);
            }
        }

        private static List<Rgba32> SortPixels(List<Rgba32> pixels, SortingType type, SortingDirection direction, int rowOrColumnIndex = 0)
        {
            IEnumerable<Rgba32> sortedPixels;

            if (type == SortingType.COLOR_FREQUENCY)
            {
                // Calculate color frequencies
                var frequencyMap = pixels.GroupBy(p => p)
                                         .ToDictionary(g => g.Key, g => g.Count());

                // Sort by frequency
                sortedPixels = pixels.OrderBy(p => frequencyMap[p]);
            }
            else
            {
                // Other sorting types
                sortedPixels = type switch
                {
                    SortingType.BRIGHTNESS => pixels.OrderBy(p => p.SortByBrightness()),
                    SortingType.HUE => pixels.OrderBy(p => p.SortByHue()),
                    SortingType.SATURATION => pixels.OrderBy(p => p.SortBySaturation()),
                    SortingType.GRAYSCALE => pixels.OrderBy(p => p.SortByGrayScale()),
                    SortingType.ALPHA => pixels.OrderBy(p => p.SortByAlpha()),
                    SortingType.RANDOM => pixels.OrderBy(p => p.SortByRandom()),
                    SortingType.WAVE => pixels.OrderBy(p => p.SortByWave(rowOrColumnIndex)),
                    _ => pixels
                };
            }

            if (direction == SortingDirection.LEFT)
            {
                sortedPixels = sortedPixels.Reverse();
            }

            return sortedPixels.ToList();

        }

        private static float SortByBrightness(this Rgba32 color)
        {
            return (color.R + color.G + color.B) / 3f;
        }

        private static float SortByHue(this Rgba32 color)
        {
            // Normalize RGB to [0, 1]
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            // Find min and max RGB values
            float max = MathF.Max(r, MathF.Max(g, b));
            float min = MathF.Min(r, MathF.Min(g, b));
            float delta = max - min;

            if (delta == 0)
            {
                return 0; // Hue is undefined for grayscale, return 0
            }

            float hue;

            // Calculate hue based on which channel is max
            if (max == r)
            {
                hue = (g - b) / delta + (g < b ? 6 : 0);
            }
            else if (max == g)
            {
                hue = (b - r) / delta + 2;
            }
            else
            {
                hue = (r - g) / delta + 4;
            }

            hue /= 6; // Normalize hue to [0, 1]

            return hue * 360f; // Return hue in degrees [0, 360]
        }

        private static float SortBySaturation(this Rgba32 color)
        {
            // Normalize RGB to [0, 1]
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            // Find min and max RGB values
            float max = MathF.Max(r, MathF.Max(g, b));
            float min = MathF.Min(r, MathF.Min(g, b));
            float delta = max - min;

            // Calculate lightness
            float lightness = (max + min) / 2f;

            // Calculate saturation
            float saturation = 0f;
            if (delta != 0)
            {
                saturation = lightness <= 0.5f ? delta / (max + min) : delta / (2f - max - min);
            }

            return saturation; // Return saturation as a float in the range [0, 1]
        }

        private static float SortByGrayScale(this Rgba32 color)
        {
            // Normalize RGB to [0, 1]
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            // Calculate grayscale using weighted average
            float grayScale = 0.2126f * r + 0.7152f * g + 0.0722f * b;

            return grayScale; // Return grayscale as a float in the range [0, 1]
        }

        private static float SortByAlpha(this Rgba32 color)
        {
            // Normalize alpha to [0, 1]
            return color.A / 255f;
        }

        private static float SortByRandom(this Rgba32 color)
        {
            // Generate a random float between 0 and 1 using a single Random instance
            return (float)_random.NextDouble();
        }

        private static float SortByWave(this Rgba32 color, int position)
        {
            // Generate a sine wave based on the pixel's position (row or column index)
            // Calculate a sine wave based on position (row or column index)

            float waveValue = MathF.Sin(position * 1.0f) * 0.5f + 0.5f; // Normalize to [0, 1]
            return waveValue;
            //eturn (float)_random.NextDouble();

        }

    }
}
