using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace VideoSteganography
{

    public class Stegano
    {
        public Bitmap image { get; set; }
        public Bitmap Watermark { get; set; }
        public Bitmap EnscryptedImage { get; set; }
        private readonly string output;
        private List<string> palette;
        private readonly Color enscryptionColour;

        

        public Stegano(string image, Color colour)
        {
            this.image=new Bitmap(image);
            enscryptionColour = colour;

            
        }

        public Stegano(string image, string watermark, Color colour, string output = "result.bmp")
        {
            this.image=new Bitmap(image);
            Watermark = new Bitmap(watermark);
            palette = GetPallete(colour);
            if (GetPixelArray(Color.Black,Watermark).Length>GetPixelArray(Color.Blue,this.image).Length) throw new IndexOutOfRangeException("ЦВЗ больше исходного изображения");
            this.output = output;
            EnscryptedImage = this.image;
            enscryptionColour = colour;
        }

        public void Decrypt()
        {
            DecryptWatermark(Color.Blue);
        }



        private int[,] GetPixelArray(Color colour, Bitmap image)
        {
            int[,] result = new int[image.Width,image.Height];
            for (int i = 0; i < image.Width; i++)
            for (int j = 0; j < image.Height; j++)
            {
                result[i,j]= colour == Color.Black
                        ?(int)image.GetPixel(i,j).GetBrightness()
                        :colour == Color.Blue
                        ? image.GetPixel(i, j).B
                        : colour == Color.Green
                        ? image.GetPixel(i, j).G
                        : image.GetPixel(i, j).R;
            }
            return result;
        }

        #region shelukhin_code



        //private List<Colour> GetColourList(Color color, Bitmap image)
        //{
        //    var pixeList = GetPixelList(color, image).Distinct();
        //    var result = new List<Colour>();
        //    int i = 0;
        //    foreach (var pixel in pixeList)
        //    {
        //        result.Add(new Colour(i++,pixel));
        //    }
        //    result.Sort();
        //    return result;
        //}

        //private int[,] GetWatermarkPixels(Bitmap image)
        //{
        //    int[,] result = new int[image.Width, image.Height];
        //    for (int i = 0; i < image.Width; i++)
        //    for (int j = 0; j < image.Height; j++)
        //    {
        //        result[i, j] = (int)image.GetPixel(i, j).GetBrightness();
        //    }
        //    return result;
        //}

        #endregion

        private List<int> GetPixelList(Color color, Bitmap image)
        {
            var result = new List<int>();
            for (int i = 0; i < image.Width; i++)
                for (int j = 0; j < image.Height; j++)
                {
                    result.Add(color == Color.Blue
                        ? image.GetPixel(i, j).B
                        : color == Color.Green
                            ? image.GetPixel(i, j).G
                            : image.GetPixel(i, j).R);
                }
            return result;
        }

        private List<string> GetPallete(Color color)
        {
            var palette = new List<string>();
            var pixels = GetPixelList(color, image).Distinct();
            List<int> sortedPixels = new List<int>();
            foreach (var pixel in pixels)
            {
                sortedPixels.Add(pixel);
            }
            sortedPixels.Sort();
            foreach (var pixel in sortedPixels)
            {
                palette.Add(Convert.ToString(pixel,2));
            }
            return palette;
        }

        private string[,] GetImageBytes(Color color,Bitmap image)
        {
            var pixels = GetPixelArray(color,image);
            string[,] result = new string[pixels.GetLength(0),pixels.GetLength(1)];
            for (int i = 0; i < pixels.GetLength(0); i++)
            for (int j = 0; j < pixels.GetLength(1); j++)
            {
                result[i, j] = Convert.ToString(pixels[i, j],2);
            }
            return result;
        }

        private void ChangeColour(ref string pixel)
        {
            pixel = palette.IndexOf(pixel) == palette.Count - 1 ? palette[palette.IndexOf(pixel) - 1] : palette[palette.IndexOf(pixel) + 1];
        }

        private void MarkEndOfFile(ref string pixel)
        {
            char[] n = pixel.ToCharArray();
            n[n.Length-2] = n[n.Length-2] == '1' ? '0' : '1';
            n[n.Length - 1] = n[n.Length-1] == '1' ? '0' : '1';
            pixel = new string(n);
        }

        private int[,] GetEnscryptedImage(Color color)
        {
            var result = GetPixelArray(color, image);
            var imageInBytes = GetImageBytes(color, image);
            var watermarkBytes = GetPixelArray(Color.Black, Watermark);
            int i;
            int j;
            for (i = 0; i < watermarkBytes.GetLength(0); i++)
            for (j = 0; j < watermarkBytes.GetLength(1); j++)
            {
                if (watermarkBytes[i, j] == 1) ChangeColour(ref imageInBytes[i,j]);
                result[i, j] = Convert.ToInt32(imageInBytes[i, j], 2);
            }
            return result;
        }

        private Color GetNewColor(Color color,int i, int j,int value)
        {
            if (color==Color.Blue) return Color.FromArgb(image.GetPixel(i,j).R,image.GetPixel(i,j).G,value);
            else if (color == Color.Green)
                return Color.FromArgb(image.GetPixel(i, j).R, value, image.GetPixel(i, j).B);
            else return Color.FromArgb(value, image.GetPixel(i, j).G, image.GetPixel(i, j).B);
        }

        private void WriteEnscryptedImage(Color color)
        {
            var enscryptedBytes = GetEnscryptedImage(color);
            for (int i = 0; i < EnscryptedImage.Width; i++)
            for (int j = 0; j < EnscryptedImage.Height; j++)
            {
                Color pixel = EnscryptedImage.GetPixel(i, j);
                EnscryptedImage.SetPixel(i,j,GetNewColor(color,i,j,enscryptedBytes[i,j]));
            }
            EnscryptedImage.Save(output);
        }

        private void DecryptWatermark(Color color)
        {
            Watermark = new Bitmap(132,132);
            var watermarkPixels = GetWatermarkPixels(color);
            for (int i = 0; i < Watermark.Width; i++)
            for (int j = 0; j<Watermark.Height; j++)
            {
                Watermark.SetPixel(i, j, watermarkPixels[i, j] == 1 ? Color.White : Color.Black);
            }
            Watermark.Save("wm.bmp");
        }
        

        private int[,] GetWatermarkPixels(Color color)
        {
            var original = GetPixelArray(color, image);
            var enscrypt = GetPixelArray(color, EnscryptedImage);
            int[,] watermarkPixels = new int[132,132];
            for (int i = 0; i < image.Width; i++)
            for (int j = 0; j < image.Height; j++)
            {
                if (original[i, j] != enscrypt[i, j]) watermarkPixels[i, j] = 1;
            }
            return watermarkPixels;
        }
    }
}
