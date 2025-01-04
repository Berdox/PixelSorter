
namespace PixelSorter.src {
    using System.Linq;

    public static class Sorters {
        public static IEnumerable<Pixel> SortByBrightness(IEnumerable<Pixel> pixels) {
            return pixels.OrderBy(p => p.Brightness);
        }

        public static IEnumerable<Pixel> SortByHue(IEnumerable<Pixel> pixels) {
            return pixels.OrderBy(p => p.Hue);
        }

        public static IEnumerable<Pixel> CustomSort(IEnumerable<Pixel> pixels, Func<Pixel, Pixel, int> comparer) {
            return pixels.OrderBy(p => p, Comparer<Pixel>.Create(new Comparison<Pixel>(comparer)));
        }
    }

}
