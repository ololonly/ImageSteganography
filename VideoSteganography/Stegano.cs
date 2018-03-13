using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;

namespace VideoSteganography
{
  public class Stegano
    {
        /// <summary>
        /// Original image
        /// </summary>
        public Bitmap Image { get; private set; }
        /// <summary>
        /// Enctypted watermark
        /// </summary>
        public Bitmap Watermark { get; private set; }
        /// <summary>
        /// Encrypted image
        /// </summary>
        public Bitmap EnscryptedImage { get; private set; }

        private List<int> palette;
        private readonly Color encryptionColour;

        /// <summary>
        /// Creating one of Stegano type object. This class can encrypt and decrypt images with watermarks using palette changing method.
        /// </summary>
        /// <param name="image">Original image</param>
        /// <param name="colour">Colour that will carry watermark</param>
        public Stegano(string image, Color colour)
        {
            this.Image=new Bitmap(image);
            encryptionColour = colour;
            palette = GetPallete();
            EnscryptedImage = this.Image;
        }

        /// <summary>
        /// Error estimation method
        /// </summary>
        /// <param name="MSE">Mean Square Error</param>
        /// <param name="NMSE">Normalized Mean Square Error</param>
        /// <param name="SNR">Signal to Noise Ratio</param>
        /// <param name="PSNR">Peak Signal to Noise Rati</param>
        public void Errors(out double MSE, out double NMSE, out double SNR, out double PSNR)
        {
            MSE = 0;
            double denominator = 0;
            var image = GetPixelArray(encryptionColour, Image);
            var encrypted = GetPixelArray(encryptionColour, EnscryptedImage);
            for (int i = 0; i < Image.Width; i++)
            {
                for (int j = 0; j < Image.Height; j++)
                {
                    MSE += Math.Pow(image[i, j] - encrypted[i, j], 2);
                    denominator += Math.Pow(image[i, j], 2);

                }
            }
            NMSE = MSE / denominator * 100;
            SNR = 10 * Math.Log10(denominator / MSE);
            MSE = MSE / image.Length;
            PSNR = 20 * Math.Log10(palette.Max() / Math.Sqrt(MSE));
        }

        public double NMSE()
        {
            double error = 0;
            var image = GetPixelArray(encryptionColour, Image);
            var encrypted = GetPixelArray(encryptionColour, EnscryptedImage);
            for (int i = 0; i < Image.Width; i++)
            {
                for (int j = 0; j < Image.Height; j++)
                {
                    error += Math.Pow(image[i, j] - encrypted[i, j], 2)/Math.Pow(image[i,j],2);
                }
            }
            return error;
        }

        public double SNR()
        {
            double error = 0;
            var image = GetPixelArray(encryptionColour, Image);
            var encrypted = GetPixelArray(encryptionColour, EnscryptedImage);
            for (int i = 0; i < Image.Width; i++)
            {
                for (int j = 0; j < Image.Height; j++)
                {
                    error += Math.Pow(image[i, j] - encrypted[i, j], 2);
                }
            }
            return error / (image.Length);
        }

        #region Palette

        private List<int> GetPixelList()
        {
            var result = new List<int>();
            for (int i = 0; i < Image.Width; i++)
            for (int j = 0; j < Image.Height; j++)
            {
                result.Add(encryptionColour == Color.Blue
                    ? Image.GetPixel(i, j).B
                    : encryptionColour == Color.Green
                        ? Image.GetPixel(i, j).G
                        : Image.GetPixel(i, j).R);
            }
            return result;
        }

        private List<int> GetPallete()
        {
            var pixels = GetPixelList().Distinct();
            List<int> sortedPixels = new List<int>();
            foreach (var pixel in pixels)
            {
                sortedPixels.Add(pixel);
            }
            sortedPixels.Sort();
            return sortedPixels;
        }

        #endregion

        /// <summary>
        /// Encrypt image with watermark.
        /// </summary>
        /// <param name="watermark">Watermark should be monochrome bmp image. Also watermark's size should not exceed original image size.</param>
        public void Encrypt(Bitmap watermark)
        {
            this.Watermark = watermark;
            if (Watermark.Height*watermark.Width > Image.Height*Image.Width) throw new IndexOutOfRangeException("ЦВЗ больше исходного изображения");
            WriteEnscryptedImage();
        }
        
        #region Encrypt
        
        private int[,] GetPixelArray(Color colour, Bitmap image)
        {
            int[,] result = new int[image.Width, image.Height];
            for (int i = 0; i < image.Width; i++)
                for (int j = 0; j < image.Height; j++)
                {
                    result[i, j] = colour == Color.Black
                            ? (int)image.GetPixel(i, j).GetBrightness()
                            : colour == Color.Blue
                            ? image.GetPixel(i, j).B
                            : colour == Color.Green
                            ? image.GetPixel(i, j).G
                            : image.GetPixel(i, j).R;
                }
            return result;
        }
        
        private int[,] GetEncryptedImage()
        {
            var result = GetPixelArray(encryptionColour, Image);
            var watermarkBytes = GetPixelArray(Color.Black, Watermark);
            int i, j = 0;
            for (i = 0; i < watermarkBytes.GetLength(0); i++)
                for (j = 0; j < watermarkBytes.GetLength(1); j++)
                {
                    if (watermarkBytes[i, j] == 1) ChangeColour(ref result[i, j]);
                }
            MarkEndOfFile(ref result[--i,--j]);
            return result;
        }

        private void ChangeColour(ref int pixel)
        {
            pixel = palette.IndexOf(pixel) == palette.Count - 1 ? palette[palette.IndexOf(pixel) - 1] : palette[palette.IndexOf(pixel) + 1];
        }

        private void MarkEndOfFile(ref int pixel)
        {
            pixel = palette.IndexOf(pixel)==palette.Count-1 ? palette[palette.IndexOf(pixel) - 3] : palette[palette.IndexOf(pixel) + 1];
        }

        private void WriteEnscryptedImage()
        {
            var enscryptedBytes = GetEncryptedImage();
            for (int i = 0; i < EnscryptedImage.Width; i++)
                for (int j = 0; j < EnscryptedImage.Height; j++)
                {
                    Color pixel = EnscryptedImage.GetPixel(i, j);
                    EnscryptedImage.SetPixel(i, j, GetNewColor(i, j, enscryptedBytes[i, j]));
                }
        }

        private Color GetNewColor(int i, int j, int value)
        {
            if (encryptionColour == Color.Blue) return Color.FromArgb(Image.GetPixel(i, j).R, Image.GetPixel(i, j).G, value);
            else if (encryptionColour == Color.Green)
                return Color.FromArgb(Image.GetPixel(i, j).R, value, Image.GetPixel(i, j).B);
            else return Color.FromArgb(value, Image.GetPixel(i, j).G, Image.GetPixel(i, j).B);
        }

        #endregion

        /// <summary>
        /// Decrypt watermark from encrypted image.
        /// </summary>
        /// <param name="encrypted">Encrypted image, should be visually equal to original image.</param>
        public void Decrypt(Bitmap encrypted)
        {
            this.EnscryptedImage = encrypted;
            if (EnscryptedImage.Width*EnscryptedImage.Height != Image.Width*Image.Height) throw new IndexOutOfRangeException("Шифрованное изображение не совпадает с оригиналом");
            DecryptWatermark();
        }

        #region Decrypt

        private void DecryptWatermark()
        {
            int width, height;
            GetWatermarkSize(out width,out height);
            Watermark = new Bitmap(width,height);
            var watermarkPixels = GetWatermarkPixels(width,height);
            for (int i = 0; i < Watermark.Width; i++)
            for (int j = 0; j < Watermark.Height; j++)
            {
                Watermark.SetPixel(i, j, watermarkPixels[i, j] == 1 ? Color.White : Color.Black);
            }
        }

        private void GetWatermarkSize(out int width, out int height)
        {
            var original = GetPixelArray(encryptionColour, Image);
            var encrypt = GetPixelArray(encryptionColour, EnscryptedImage);
            for (int i = 0; i < encrypt.GetLength(0); i++)
            for (int j = 0; j < encrypt.GetLength(1); j++)
            {
                if (Math.Abs(palette.IndexOf(original[i, j]) - palette.IndexOf(encrypt[i, j])) == 2)
                {
                    width = ++i;
                    height = ++j;
                    return;
                 }
            }
            width = 0;
            height = 0;
            throw new Exception("В данном файле изображения нет ЦВЗ");
        }

        private int[,] GetWatermarkPixels(int width, int height)
        {
            var original = GetPixelArray(encryptionColour, Image);
            var encrypt = GetPixelArray(encryptionColour, EnscryptedImage);
            
            int[,] watermarkPixels = new int[width,height];
            for (int i = 0; i < Image.Width; i++)
            for (int j = 0; j < Image.Height; j++)
            {
                if (original[i, j] != encrypt[i, j]) watermarkPixels[i, j] = 1;
            }
            return watermarkPixels;
        }

        #endregion
    }
}
