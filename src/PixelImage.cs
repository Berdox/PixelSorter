using OpenCvSharp;

namespace PixelSorter.src {

    public class PixelImage {
        private Mat _image;

        public int Width => _image.Cols;
        public int Height => _image.Rows;

        public PixelImage(string filePath) {
            _image = Cv2.ImRead(filePath, ImreadModes.Color);
        }

        public void Save(string filePath) {
            Cv2.ImWrite(filePath, _image);
        }

        public Pixel GetPixel(int x, int y) {
            Vec3b color = _image.At<Vec3b>(y, x);
            return new Pixel(x, y, new Color(color[2], color[1], color[0])); // Convert BGR to RGB
        }

        public void SetPixel(Pixel pixel) {
            _image.Set(pixel.Y, pixel.X, value: new Vec3b(pixel.Color.B, pixel.Color.G, pixel.Color.R));
        }
    }

}
