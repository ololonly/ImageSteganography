using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace VideoSteganography
{

    public class Stegano
    {
        public Bitmap Image { get; private set; }
        public Bitmap Watermark { get; private set; }
        public Bitmap EnscryptedImage { get; private set; }
        private List<string> palette;
        private readonly Color encryptionColour;

        public Stegano(string image, Color colour)
        {
            this.Image=new Bitmap(image);
            encryptionColour = colour;
            palette = GetPallete();
            EnscryptedImage = this.Image;

        }
        
        public void Encrypt(Bitmap watermark)
        {
            this.Watermark = watermark;
            if (GetPixelArray(Color.Black, Watermark).Length > GetPixelArray(Color.Blue, this.Image).Length) throw new IndexOutOfRangeException("ЦВЗ больше исходного изображения");
            WriteEnscryptedImage();
        }

        #region Palette

        private List<int> GetPixelList(Bitmap image)
        {
            var result = new List<int>();
            for (int i = 0; i < image.Width; i++)
            for (int j = 0; j < image.Height; j++)
            {
                result.Add(encryptionColour == Color.Blue
                    ? image.GetPixel(i, j).B
                    : encryptionColour == Color.Green
                        ? image.GetPixel(i, j).G
                        : image.GetPixel(i, j).R);
            }
            return result;
        }

        private List<string> GetPallete()
        {
            var palette = new List<string>();
            var pixels = GetPixelList(Image).Distinct();
            List<int> sortedPixels = new List<int>();
            foreach (var pixel in pixels)
            {
                sortedPixels.Add(pixel);
            }
            sortedPixels.Sort();
            foreach (var pixel in sortedPixels)
            {
                palette.Add(Convert.ToString(pixel, 2));
            }
            return palette;
        }

        #endregion

        #region Encrypt




        #endregion


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

       

        private string[,] GetImageBytes(Bitmap image)
        {
            var pixels = GetPixelArray(encryptionColour,image);
            string[,] result = new string[pixels.GetLength(0),pixels.GetLength(1)];
            for (int i = 0; i < pixels.GetLength(0); i++)
            for (int j = 0; j < pixels.GetLength(1); j++)
            {
                result[i, j] = Convert.ToString(pixels[i, j],2);
            }
            return result;
        }
        
        private int[,] GetEncryptedImage()
        {
            var result = GetPixelArray(encryptionColour, Image);
            var imageInBytes = GetImageBytes(Image);
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

        private void ChangeColour(ref string pixel)
        {
            pixel = palette.IndexOf(pixel) == palette.Count - 1 ? palette[palette.IndexOf(pixel) - 1] : palette[palette.IndexOf(pixel) + 1];
        }

        private Color GetNewColor(int i, int j,int value)
        {
            if (encryptionColour==Color.Blue) return Color.FromArgb(Image.GetPixel(i,j).R,Image.GetPixel(i,j).G,value);
            else if (encryptionColour == Color.Green)
                return Color.FromArgb(Image.GetPixel(i, j).R, value, Image.GetPixel(i, j).B);
            else return Color.FromArgb(value, Image.GetPixel(i, j).G, Image.GetPixel(i, j).B);
        }

        private void WriteEnscryptedImage()
        {
            var enscryptedBytes = GetEncryptedImage();
            for (int i = 0; i < EnscryptedImage.Width; i++)
            for (int j = 0; j < EnscryptedImage.Height; j++)
            {
                Color pixel = EnscryptedImage.GetPixel(i, j);
                EnscryptedImage.SetPixel(i,j,GetNewColor(i,j,enscryptedBytes[i,j]));
            }
        }

        public void Decrypt(Bitmap encrypted)
        {
            this.EnscryptedImage = encrypted;
            if (GetPixelArray(Color.Black, EnscryptedImage).Length != GetPixelArray(Color.Blue, this.Image).Length) throw new IndexOutOfRangeException("Шифрованное изображение не совпадает с оригиналом");
            DecryptWatermark();
        }
        #region Decrypt

        private void DecryptWatermark()
        {
            Watermark = new Bitmap(132,132);
            var watermarkPixels = GetWatermarkPixels();
            for (int i = 0; i < Watermark.Width; i++)
            for (int j = 0; j<Watermark.Height; j++)
            {
                Watermark.SetPixel(i, j, watermarkPixels[i, j] == 1 ? Color.White : Color.Black);
            }
        }
        
        private int[,] GetWatermarkPixels()
        {
            var original = GetPixelArray(encryptionColour, Image);
            var enscrypt = GetPixelArray(encryptionColour, EnscryptedImage);
            int[,] watermarkPixels = new int[132,132];
            for (int i = 0; i < Image.Width; i++)
            for (int j = 0; j < Image.Height; j++)
            {
                if (original[i, j] != enscrypt[i, j]) watermarkPixels[i, j] = 1;
            }
            return watermarkPixels;

        #endregion
       
        }
    }
}
