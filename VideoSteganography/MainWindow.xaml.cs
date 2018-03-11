using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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
            fileDialog.Filter = Filter;

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
            steganoButton.Click += (s, e) =>
            {
                Stegano iStegano = new Stegano(imageSearchTextBox.Text,watermarkSearchTextBox.Text);
            };
        }
    }
}
