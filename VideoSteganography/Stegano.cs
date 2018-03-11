using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VideoSteganography
{
    enum Colours
    {
        Monochrome,
        Red,
        Green,
        Blue
    }

    public class Stegano
    {
        private Bitmap image;
        private Bitmap watermark;
        private const string endOfWatermark = "end";

        public Stegano(string image, string watermark)
        {
            this.image=new Bitmap(image);
            this.watermark = new Bitmap(watermark);
            var test1 = GetPixelArray(Colours.Blue, this.image);
            var test2 = GetColourList(Colours.Blue, this.image);
 //           var test3 = GetWatermarkPixels(this.watermark);
            var test4 = GetWatermarkBytes(this.watermark);
        }

        private int[,] GetPixelArray(Colours colour, Bitmap image)
        {
            int[,] result = new int[image.Width,image.Height];
            for (int i = 0; i < image.Width; i++)
            for (int j = 0; j < image.Height; j++)
            {
                result[i,j]=colour == Colours.Blue
                    ? image.GetPixel(i, j).B
                    : colour == Colours.Green
                        ? image.GetPixel(i, j).G
                        : image.GetPixel(i, j).R;
            }
            return result;
        }

        private List<int> GetPixelList(Colours colour, Bitmap image)
        {
            var result = new List<int>();
            for (int i = 0; i < image.Width; i++)
            for (int j = 0; j < image.Height; j++)
            {
                result.Add(colour == Colours.Blue
                    ? image.GetPixel(i, j).B
                    : colour == Colours.Green
                        ? image.GetPixel(i, j).G
                        : image.GetPixel(i, j).R);
            }
            return result;
        }

        private List<Colour> GetColourList(Colours colour, Bitmap image)
        {
            var pixeList = GetPixelList(colour, image).Distinct();
            var result = new List<Colour>();
            int i = 0;
            foreach (var pixel in pixeList)
            {
                result.Add(new Colour(i++,pixel));
            }
            result.Sort();
            return result;
        }

        private int[,] GetWatermarkPixels(Bitmap image)
        {
            int[,] result = new int[image.Width, image.Height];
            for (int i = 0; i < image.Width; i++)
            for (int j = 0; j < image.Height; j++)
            {
                result[i, j] = (int)image.GetPixel(i, j).GetBrightness();
            }
            return result;
        }

        private byte[] GetWatermarkBytes(Bitmap image)
        {
            var pixels = GetWatermarkPixels(image);
            byte[] result = new byte[pixels.Length+endOfWatermark.Length];
            int i = 0;
            foreach (var pixel in pixels)
            {
                result[i] = (byte)pixel;
                i++;
            }
            foreach (var character in endOfWatermark)
            {
                result[i] = (byte) character;
                i++;
            }
            return result;
        }


    }
}
