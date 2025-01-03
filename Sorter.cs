using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Numerics;

namespace PixelSorter
{
    public enum SortingType
    {
        BRIGHTNESS,
        HUE,
        SATURATION,
        GRAYSCALE,
        ALPHA,
        RANDOM,
        WAVE
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

    public static class Sorter {

        // 1. Get image
        // 2. are you sorting by row, col, or diaginaol.
        // 3. Then also left or right for col and row. For diaginal starting position top left, top right, bottom right, or bottom left.
        // 4. what type of sorting are you doing (brightness, hue, saturation, Luminance, Alpha, grayscale, Graident, random)
        // 5. Ex. Luminace are we going from brightest to dimest or vice versa

        private static Random _random = new Random();

        public static void SortByRow(string inputPath, string outputPath, SortingType type, SortingDirection direction) {

            Image<Rgba32> image;

            try {
                image = ReadImage(inputPath);
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to load image: {ex.Message}");
                return;
            }

            for (int y = 0; y < image.Width; y++) {

                var rowPixels = new List<Rgba32>();
                for (int x = 0; x < image.Height; x++) {
                    rowPixels.Add(image[x, y]);
                }

                var sortedPixels = SortPixels(rowPixels, type, direction, y);

                for (int x = 0; x < image.Height; x++) {
                    image[x, y] = sortedPixels[x];
                }
            }

            image.Save(outputPath);
            Console.WriteLine("Image saved to " + outputPath);
        }

        public static void SortRowByMask(SortingType type, SortingDirection direction, string imagePath, string outputPath, string maskPath = "", 
                                                                            float lowthreshold = 0.0f, float highThreshold = 1.0f, float contrastFacter = 1.0f) {

            Image<Rgba32> image, contrastImg, edgesImg;

            // Tries to load image
            try {
                image = ReadImage(imagePath);
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to load image: {ex.Message}");
                return;
            }

            // Generates a mask based in thresholds or loads a provided mask
            if (maskPath != "") {
                try {
                    contrastImg = ReadImage(maskPath);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to load image: {ex.Message}");
                    return;
                }
            } else {
                contrastImg = GenerateBlackAndWhiteMask(image, lowthreshold, highThreshold, contrastFacter);
            }

            // Checks to see if image and mask are same size
            if (image.Height != contrastImg.Height || image.Width != contrastImg.Width) {
                throw new InvalidOperationException(
                    $"Image and Mask need to be the same size. Image size: {image.Width}x{image.Height}, Mask size: {contrastImg.Width}x{contrastImg.Height}"
                );
            }

            edgesImg = image.Clone();
            edgesImg.Mutate(x => x.Grayscale()
                                    .GaussianBlur(2)
                                    .BinaryThreshold(0.5f)
                                    .DetectEdges());



            // Passes row by row of pixels to be sorted
            for (int y = 0; y < image.Height; y++) {

                var rowPixels = new List<Rgba32>();
                var rowMaskPixels = new List<Rgba32>();
                var rowEdgePixels = new List<Rgba32>();
                for (int x = 0; x < image.Width; x++) {
                    rowPixels.Add(image[x, y]);
                    rowMaskPixels.Add(contrastImg[x, y]);
                    rowEdgePixels.Add(edgesImg[x, y]);
                }

                var sortedPixels = SortPixels(rowPixels, rowMaskPixels, rowEdgePixels, type, direction, y);

                for (int x = 0; x < image.Width; x++) {
                    image[x, y] = sortedPixels[x];
                }
            }

            // Saves the out of the image
            try {
                image.Save(outputPath);
                //contrastImg.Save(outputPath);
                //edgesImg.Save(outputPath);
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to load image: {ex.Message}");
            }

            Console.WriteLine("Image saved to " + outputPath);

        }

        public static void SortByColumn(string inputPath, string outputPath, SortingType type, SortingDirection direction) {

            Image<Rgba32> image;

            try {
                image = ReadImage(inputPath);
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to load image: {ex.Message}");
                return;
            }

            for (int x = 0; x < image.Width; x++) {

                var rowPixels = new List<Rgba32>();
                for (int y = 0; y < image.Height; y++) {
                    rowPixels.Add(image[x, y]);
                }

                var sortedPixels = SortPixels(rowPixels, type, direction, x);

                for (int y = 0; y < image.Height; y++) {
                    image[x, y] = sortedPixels[y];
                }
            }

            image.Save(outputPath);
            Console.WriteLine("Image saved to " + outputPath);
        }

        public static void SortColumnByMask(SortingType type, SortingDirection direction, string imagePath, string outputPath, string maskPath = "",
                                                                            float lowthreshold = 0.0f, float highThreshold = 1.0f, float contrastFacter = 1.0f) {

            Image<Rgba32> image, contrastImg, edgesImg;

            // Tries to load image
            try {
                image = ReadImage(imagePath);
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to load image: {ex.Message}");
                return;
            }

            // Generates a mask based in thresholds or loads a provided mask
            if (maskPath != "") {
                try {
                    contrastImg = ReadImage(maskPath);
                }
                catch (Exception ex) {
                    Console.WriteLine($"Failed to load image: {ex.Message}");
                    return;
                }
            }
            else {
                contrastImg = GenerateBlackAndWhiteMask(image, lowthreshold, highThreshold, contrastFacter);
            }

            // Checks to see if image and mask are same size
            if (image.Height != contrastImg.Height || image.Width != contrastImg.Width) {
                throw new InvalidOperationException(
                    $"Image and Mask need to be the same size. Image size: {image.Width}x{image.Height}, Mask size: {contrastImg.Width}x{contrastImg.Height}"
                );
            }

            edgesImg = image.Clone();
            edgesImg.Mutate(x => x.Grayscale()
                                    .GaussianBlur(2)
                                    .BinaryThreshold(0.5f)
                                    .DetectEdges());



            // Passes row by row of pixels to be sorted
            for (int x = 0; x < image.Width; x++) {

                var rowPixels = new List<Rgba32>();
                var rowMaskPixels = new List<Rgba32>();
                var rowEdgePixels = new List<Rgba32>();
                for (int y = 0; y < image.Height; y++) {
                    rowPixels.Add(image[x, y]);
                    rowMaskPixels.Add(contrastImg[x, y]);
                    rowEdgePixels.Add(edgesImg[x, y]);
                }

                var sortedPixels = SortPixels(rowPixels, rowMaskPixels, rowEdgePixels, type, direction, x);

                for (int y = 0; y < image.Height; y++) {
                    image[x, y] = sortedPixels[y];
                }
            }

            // Saves the out of the image
            try {
                image.Save(outputPath);
                //contrastImg.Save(outputPath);
                //edgesImg.Save(outputPath);
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to load image: {ex.Message}");
            }

            Console.WriteLine("Image saved to " + outputPath);

        }

        public static void SortByDiagonal(string inputPath, string outputPath, SortingType type, SortingDirection direction, StartingPoint startingPoint) {
            Image<Rgba32> image;

            try {
                image = ReadImage(inputPath);
            }
            catch (Exception ex) {
                Console.WriteLine($"Failed to load image: {ex.Message}");
                return;
            }

            int width = image.Width;
            int height = image.Height;

            // Iterate over all diagonals based on the starting point
            for (int d = 0; d < width + height - 1; d++) {
                var diagonalPixels = new List<Rgba32>();

                // Collect pixels based on the starting point
                if (startingPoint == StartingPoint.TopLeft) {
                    for (int x = 0; x < width; x++) {
                        int y = d - x;
                        if (y >= 0 && y < height) {
                            diagonalPixels.Add(image[x, y]);
                        }
                    }
                }
                else if (startingPoint == StartingPoint.TopRight) {
                    for (int x = 0; x < width; x++) {
                        int y = d - (width - 1 - x);
                        if (y >= 0 && y < height) {
                            diagonalPixels.Add(image[x, y]);
                        }
                    }
                }
                else if (startingPoint == StartingPoint.BottomLeft) {
                    for (int x = 0; x < width; x++) {
                        int y = height - 1 - d + x;
                        if (y >= 0 && y < height) {
                            diagonalPixels.Add(image[x, y]);
                        }
                    }
                }
                else if (startingPoint == StartingPoint.BottomRight) {
                    for (int x = 0; x < width; x++) {
                        int y = height - 1 - d + (width - 1 - x);
                        if (y >= 0 && y < height) {
                            diagonalPixels.Add(image[x, y]);
                        }
                    }
                }

                // Sort the pixels
                var sortedPixels = SortPixels(diagonalPixels, type, direction);

                // Place the sorted pixels back into the diagonal
                int index = 0;
                if (startingPoint == StartingPoint.TopLeft || startingPoint == StartingPoint.TopRight) {
                    for (int x = 0; x < width; x++) {
                        int y = startingPoint == StartingPoint.TopLeft ? d - x : d - (width - 1 - x);
                        if (y >= 0 && y < height) {
                            image[x, y] = sortedPixels[index++];
                        }
                    }
                }
                else if (startingPoint == StartingPoint.BottomLeft || startingPoint == StartingPoint.BottomRight) {
                    for (int x = 0; x < width; x++) {
                        int y = startingPoint == StartingPoint.BottomLeft ? height - 1 - d + x : height - 1 - d + (width - 1 - x);
                        if (y >= 0 && y < height) {
                            image[x, y] = sortedPixels[index++];
                        }
                    }
                }
            }

            image.Save(outputPath);
            Console.WriteLine("Image saved to " + outputPath);
        }

        private static Image<Rgba32> ReadImage(string inputPath) {
            Console.WriteLine("Current directory: " + Directory.GetCurrentDirectory());

            // Check if file exists
            if (!File.Exists(inputPath)) {
                throw new FileNotFoundException($"The file at {inputPath} does not exist.");
            }

            try {
                return Image.Load<Rgba32>(inputPath);
            }
            catch (ImageProcessingException ex) {
                throw new InvalidOperationException($"Error processing the image: {ex.Message}", ex);
            }
        }

        private static Image<Rgba32> GenerateBlackAndWhiteMask(Image<Rgba32> image, float lowThreshold = 0.0f, float highThreshold = 1.0f, float contrastFactor = 1.0f) {
            // Ensure thresholds are within the valid [0, 1] range
            if (lowThreshold < 0 || lowThreshold > 1) {
                throw new ArgumentOutOfRangeException(nameof(lowThreshold), "Low threshold must be between 0 and 1.");
            }
            if (highThreshold < 0 || highThreshold > 1) {
                throw new ArgumentOutOfRangeException(nameof(highThreshold), "High threshold must be between 0 and 1.");
            }
            if (lowThreshold > highThreshold) {
                throw new ArgumentException("Low threshold must be less than or equal to high threshold.");
            }

            // Step 2: Convert to Grayscale (Luminance)
            var grayscaleImage = image.Clone(ctx => ctx.Grayscale());

            // Step 3: Enhance contrast by manually adjusting brightness (basic contrast boost)
            grayscaleImage.Mutate(ctx => ctx.Contrast(contrastFactor)); // Adjust contrast factor as needed

            // Step 5: Apply thresholding based on normalized intensity and thresholds
            for (int y = 0; y < grayscaleImage.Height; y++) {
                for (int x = 0; x < grayscaleImage.Width; x++) {
                    var pixel = grayscaleImage[x, y];
                    // Normalize intensity (average of RGB channels)
                    float intensity = ((pixel.R + pixel.G + pixel.B) / 3.0f) / 255.0f;

                    // Check if intensity is between low and high thresholds
                    if (intensity >= lowThreshold && intensity <= highThreshold) {
                        grayscaleImage[x, y] = new Rgba32(1f, 1f, 1f); // White (normalized to [0, 1])
                    }
                    else {
                        grayscaleImage[x, y] = new Rgba32(0f, 0f, 0f); // Black (normalized to [0, 0])
                    }
                }
            }

            return grayscaleImage;
        }

        private static List<Rgba32> SortPixels(List<Rgba32> pixels, SortingType type, SortingDirection direction, int rowOrColumnIndex = 0) {
            IEnumerable<Rgba32> sortedPixels;

            // Other sorting types
            sortedPixels = type switch {
                SortingType.BRIGHTNESS => pixels.OrderBy(p => p.SortByBrightness()),
                SortingType.HUE => pixels.OrderBy(p => p.SortByHue()),
                SortingType.SATURATION => pixels.OrderBy(p => p.SortBySaturation()),
                SortingType.GRAYSCALE => pixels.OrderBy(p => p.SortByGrayScale()),
                SortingType.ALPHA => pixels.OrderBy(p => p.SortByAlpha()),
                SortingType.RANDOM => pixels.OrderBy(p => p.SortByRandom()),
                SortingType.WAVE => pixels.OrderBy(p => p.SortByWave(rowOrColumnIndex)),
                _ => pixels
            };

            if (direction == SortingDirection.LEFT) {
                sortedPixels = sortedPixels.Reverse();
            }

            return sortedPixels.ToList();

        }

        private static List<Rgba32> SortPixels(List<Rgba32> pixels, List<Rgba32> maskPixels, List<Rgba32> edgePixels, SortingType type, SortingDirection direction, int rowOrColumnIndex = 0) {
            // Create a list to store only pixels where the corresponding mask or edge pixel is white
            var sortablePixels = pixels.Select((pixel, index) => new { pixel, index, maskPixel = maskPixels[index], edgePixel = edgePixels[index] })
                                       .Where(p =>
                                           (p.maskPixel.R == 255 && p.maskPixel.G == 255 && p.maskPixel.B == 255) || // White mask pixel
                                           (p.edgePixel.R == 255 && p.edgePixel.G == 255 && p.edgePixel.B == 255)    // White edge pixel
                                       )
                                       .ToList();

            // Sort the sortable pixels based on the sorting type
            IEnumerable<Rgba32> sortedResult = type switch {
                SortingType.BRIGHTNESS => sortablePixels.OrderBy(p => p.pixel.SortByBrightness()).Select(p => p.pixel),
                SortingType.HUE => sortablePixels.OrderBy(p => p.pixel.SortByHue()).Select(p => p.pixel),
                SortingType.SATURATION => sortablePixels.OrderBy(p => p.pixel.SortBySaturation()).Select(p => p.pixel),
                SortingType.GRAYSCALE => sortablePixels.OrderBy(p => p.pixel.SortByGrayScale()).Select(p => p.pixel),
                SortingType.ALPHA => sortablePixels.OrderBy(p => p.pixel.SortByAlpha()).Select(p => p.pixel),
                SortingType.RANDOM => sortablePixels.OrderBy(p => p.pixel.SortByRandom()).Select(p => p.pixel),
                SortingType.WAVE => sortablePixels.OrderBy(p => p.pixel.SortByWave(rowOrColumnIndex)).Select(p => p.pixel),
                _ => sortablePixels.Select(p => p.pixel),
            };

            if (direction == SortingDirection.LEFT) {
                sortedResult = sortedResult.Reverse();
            }

            // Rebuild the final list with sorted pixels for white mask or edge pixels, keeping others in their original positions
            var resultPixels = pixels.ToList();
            var sortedPixelsList = sortedResult.ToList();
            int sortedIndex = 0;

            for (int i = 0; i < pixels.Count; i++) {
                if ((maskPixels[i].R == 255 && maskPixels[i].G == 255 && maskPixels[i].B == 255) || // Check if mask is white
                    (edgePixels[i].R == 255 && edgePixels[i].G == 255 && edgePixels[i].B == 255)) { // Check if edge is white
                    resultPixels[i] = sortedPixelsList[sortedIndex++];
                }
            }

            return resultPixels;
        }

        private static float SortByBrightness(this Rgba32 color) {
            return (color.R + color.G + color.B) / 3f;
        }

        private static float SortByHue(this Rgba32 color) {
            // Normalize RGB to [0, 1]
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            // Find min and max RGB values
            float max = MathF.Max(r, MathF.Max(g, b));
            float min = MathF.Min(r, MathF.Min(g, b));
            float delta = max - min;

            if (delta == 0) {
                return 0; // Hue is undefined for grayscale, return 0
            }

            float hue;

            // Calculate hue based on which channel is max
            if (max == r) {
                hue = (g - b) / delta + (g < b ? 6 : 0);
            }
            else if (max == g) {
                hue = (b - r) / delta + 2;
            }
            else {
                hue = (r - g) / delta + 4;
            }

            hue /= 6; // Normalize hue to [0, 1]

            return hue * 360f; // Return hue in degrees [0, 360]
        }

        private static float SortBySaturation(this Rgba32 color) {
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
            if (delta != 0) {
                saturation = lightness <= 0.5f ? delta / (max + min) : delta / (2f - max - min);
            }

            return saturation; // Return saturation as a float in the range [0, 1]
        }

        private static float SortByGrayScale(this Rgba32 color) {
            // Normalize RGB to [0, 1]
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            // Calculate grayscale using weighted average
            float grayScale = 0.2126f * r + 0.7152f * g + 0.0722f * b;

            return grayScale; // Return grayscale as a float in the range [0, 1]
        }

        private static float SortByAlpha(this Rgba32 color) {
            // Normalize alpha to [0, 1]
            return color.A / 255f;
        }

        private static float SortByRandom(this Rgba32 color) {
            // Generate a random float between 0 and 1 using a single Random instance
            return (float)_random.NextDouble();
        }

        private static float SortByWave(this Rgba32 color, int position) {
            // Generate a sine wave based on the pixel's position (row or column index)
            // Calculate a sine wave based on position (row or column index)

            float waveValue = MathF.Sin(position * 1.0f) * 0.5f + 0.5f; // Normalize to [0, 1]
            return waveValue;
            //eturn (float)_random.NextDouble();

        }

    }
}
