using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace VideoSteganography
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string Filter = "Image Files (*.bmp) | *.bmp";
        public MainWindow()
        {
            InitializeComponent();
            OpenFileDialog fileDialog = new OpenFileDialog();
            SaveFileDialog saveDialog = new SaveFileDialog();
            Bitmap watermark = null;
            Bitmap encryptedImage = null;
            fileDialog.Filter = Filter;
            saveDialog.Filter = Filter;

            #region CheckBox
            cryptCheckBox.Checked += (s, e) =>
            {
                //ON
                watermarkSearchTextBox.Visibility = Visibility.Visible;
                watermarkSearchButton.Visibility = Visibility.Visible;
                encryptButton.IsEnabled = true;
                //OFF
                encryptedPictureBox.Source = null;
                encryptedSearchTextBox.Text = string.Empty;
                encryptedSearchTextBox.Visibility = Visibility.Collapsed;
                encryptedSearchButton.Visibility = Visibility.Collapsed;
                encryptedSaveButton.Visibility = Visibility.Collapsed;
                watermarkSaveButton.Visibility = Visibility.Collapsed;
                decryptButton.IsEnabled = false;
            };
            cryptCheckBox.Unchecked += (s,e) => 
            {
                //OFF
                watermarkSearchTextBox.Text = string.Empty;
                watermarkPictureBox.Source = null;
                watermarkSearchTextBox.Visibility = Visibility.Collapsed;
                watermarkSearchButton.Visibility = Visibility.Collapsed;
                watermarkSaveButton.Visibility = Visibility.Collapsed;
                encryptedSaveButton.Visibility = Visibility.Collapsed;
                encryptButton.IsEnabled = false;
                //ON
                encryptedSearchTextBox.Visibility = Visibility.Visible;
                encryptedSearchButton.Visibility = Visibility.Visible;
                decryptButton.IsEnabled = true;
            };
            cryptCheckBox.IsChecked = true;
            #endregion

            watermarkSearchButton.Click += (s, e) =>
            {
                if (fileDialog.ShowDialog() == true)
                {
                    watermarkSearchTextBox.Text = fileDialog.FileName;
                    BitmapImage image = new BitmapImage(new Uri(fileDialog.FileName));
                    watermarkPictureBox.Source = image;
                }
            };
            imageSearchButton.Click += (s, e) =>
            {
                if (fileDialog.ShowDialog() == true)
                {
                    imageSearchTextBox.Text = fileDialog.FileName;
                }
                BitmapImage image = new BitmapImage(new Uri(fileDialog.FileName));
                imagePictureBox.Source = image;
            };
            encryptedSearchButton.Click += (s, e) =>
            {
                if (fileDialog.ShowDialog() == true)
                {
                    encryptedSearchTextBox.Text = fileDialog.FileName;
                }
                BitmapImage image = new BitmapImage(new Uri(fileDialog.FileName));
                encryptedPictureBox.Source = image;
            };

            encryptButton.Click += (s, e) =>
            {
                Stegano iStegano = new Stegano(imageSearchTextBox.Text,Color.Blue);
                iStegano.Encrypt(new Bitmap(watermarkSearchTextBox.Text));
                encryptedImage = iStegano.EnscryptedImage;
                encryptedPictureBox.Source = convertFromBitmap(encryptedImage);
                encryptedSaveButton.Visibility = Visibility.Visible;
            };
            encryptedSaveButton.Click += (s, e) =>
            {
                saveDialog.ShowDialog();
                if (saveDialog.FileName != string.Empty && encryptedImage!=null)
                {
                    using (FileStream fs = saveDialog.OpenFile() as FileStream)
                    {
                        encryptedImage.Save(fs,ImageFormat.Bmp);
                    }
                }
            };

            decryptButton.Click += (s, e) =>
            {
                Stegano iStegano = new Stegano(imageSearchTextBox.Text,Color.Blue);
                iStegano.Decrypt(new Bitmap(encryptedSearchTextBox.Text));
                watermark = iStegano.Watermark;
                watermarkPictureBox.Source = convertFromBitmap(watermark);
                watermarkSaveButton.Visibility = Visibility.Visible;
            };
            watermarkSaveButton.Click += (s, e) =>
            {
                saveDialog.ShowDialog();
                if (saveDialog.FileName != string.Empty && watermark != null)
                {
                    using (FileStream fs = saveDialog.OpenFile() as FileStream)
                    {
                        watermark.Save(fs, ImageFormat.Bmp);
                    }
                }
            };
        }

        private BitmapImage convertFromBitmap(Bitmap image)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                image.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
