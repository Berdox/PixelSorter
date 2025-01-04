namespace PixelSorter.src {

    public struct Color {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public byte A { get; set; }

        public float Brightness => (R + G + B) / 3f;

        public float Hue {
            get {
                float max = Math.Max(R, Math.Max(G, B));
                float min = Math.Min(R, Math.Min(G, B));

                if (max == min) return 0;

                float hue = 0;
                if (max == R)
                    hue = (G - B) / (max - min);
                else if (max == G)
                    hue = 2f + (B - R) / (max - min);
                else
                    hue = 4f + (R - G) / (max - min);

                hue *= 60;
                if (hue < 0) hue += 360;

                return hue;
            }
        }

        public Color(byte r, byte g, byte b, byte a = 255) {
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }


    public class Pixel {
        public int X { get; set; }
        public int Y { get; set; }
        public Color Color { get; set; }

        public float Brightness => Color.Brightness;
        public float Hue => Color.Hue;

        public Pixel(int x, int y, Color color) {
            X = x;
            Y = y;
            Color = color;
        }
    }

}
