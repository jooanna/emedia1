using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using Image = Windows.UI.Xaml.Controls.Image;

namespace emedia1.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;

        public MainPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _selectedFile = new Image { Source = new BitmapImage(new Uri("ms-appx:///Assets/owl.jpg")) };
        }

        private Image _selectedFile;

        public Image SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                if (_selectedFile == value) return;
                _selectedFile = value;
                RaisePropertyChanged();

            }
        }

        private Image _transformedFile = new Image();

        public Image TransformedFile
        {
            get { return _transformedFile; }
            set
            {
                if (_transformedFile == value) return;
                _transformedFile = value;
                RaisePropertyChanged();

            }
        }


        public string FilePath { get; set; } = @"ms-appx:///Assets/owl.jpg";

        public ICommand SelectFile => new RelayCommand(async () =>
        {
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");

            var file = await openPicker.PickSingleFileAsync();

            if (file == null) return;
            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            var bmp = new BitmapImage();
            bmp.SetSource(stream);
            SelectedFile.Source = bmp;
            FilePath = file.Path;
        });

        public ICommand GoToDetailPage => new RelayCommand(() =>
        {
            Messenger.Default.Send(FilePath);
            _navigationService.NavigateTo("DetailPage");
        });

        public ICommand ShowSpectrum => new RelayCommand(async () =>
        {
            await OnShowSpectrum();
        });

        public async Task CreateSpectrumImage(List<List<int>> spectrum)
        {
            List<byte[]> listOfArraysOfBytes = new List<byte[]>();
            foreach (var listOfInts in spectrum)
            {
                List<byte> listOfBytes = new List<byte>();
                foreach (var ints in listOfInts)
                {
                    listOfBytes.Add(Convert.ToByte(ints));
                }
                listOfArraysOfBytes.Add(listOfBytes.ToArray());

            }

            byte[] array = listOfArraysOfBytes
                .SelectMany(a => a)
                .ToArray();
            var bitMap = ConvertToPicture(array);
            TransformedFile.Source = await bitMap;
        }

        public async Task<BitmapImage> ConvertToPicture(byte[] imageArray)
        {
            try
            {
                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        writer.WriteBytes(imageArray);
                        await writer.StoreAsync();
                    }
                    var image = new BitmapImage();
                    await image.SetSourceAsync(stream);
                    return image;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("An error occured while calling ConvertToPicture method: " +
                                                   ex.Message);
                return null;
            }
        }

        public async Task OnShowSpectrum()
        {
            FileParser fileParser = new FileParser();
            byte[] data = await fileParser.GetResource(FilePath);
            var convertToHexa = fileParser.ConvertToHexa(data);
            var imageParser = new ImageParser(MarkerSplitter.splitMarkers(convertToHexa));
            imageParser.ParsedImage1.PathToImage = FilePath;

            ParsedImage parsedImage = await imageParser.ParseToImageInHexa();
            imageParser.ParsedImage1 = parsedImage;
            var height = imageParser.ParsedImage1.ImageHeight;
            var width = imageParser.ParsedImage1.ImageWidth;
            var coloredImage = imageParser.ParsedImage1.ImagePixels;
            Dfft(height, width, coloredImage);
        }

        public void FftShift(int height, int width, List<List<int>> greyTab, List<List<int>> coloredTab)
        {
            if (height > 50)
            {
                height = 50;
            }
            if (width > 50)
            {
                width = 50;
            }
            for (var v = 0; v < height; ++v) // jakas niby konwersja, zeby niskie czestotliwosci byly w srodku, czyli tak jak kiedys na zajeciach mielismy te obrazki
            {
                var column = 0;
                for (var u = 0; u < width; ++u)
                {
                    var pixelValue = greyTab[v][u] * (int)Math.Pow(-1, v + u);
                    coloredTab[v][column] = pixelValue;
                    coloredTab[v][++column] = pixelValue;
                    coloredTab[v][++column] = pixelValue;
                }
            }
        }

        public void Dfft(int height, int width, List<List<int>> coloredImage)
        {
            if (height > 50)
            {
                height = 50;
            }
            if (width > 50)
            {
                width = 50;
            }
            List<List<int>> grayImage = new List<List<int>>();
            for (var r = 0; r < height; ++r)
            {
                grayImage.Add(new List<int>());
                var column = 0;
                for (var c = 0; c < width * 3; ++c)
                {
                    var greyPixel = 0.2989 * coloredImage[r][c] + 0.5870 * coloredImage[r][++c] + 0.1140 * coloredImage[r][++c];
                    grayImage[r].Add((int)greyPixel);
                }
            }
            var reciprocalSquareOfWidthHeightImage = 1 / (Math.Sqrt(width * height));
            FftShift(height, width, grayImage, coloredImage);
            var maxModulValue = 0;

            for (var v = 0; v < height; ++v)
            {
                var column = 0;
                for (var u = 0; u < width; ++u)
                {
                    double sumReal = 0;
                    double sumImag = 0;
                    for (var n = 0; n < height; ++n)
                    {
                        for (var m = 0; m < width; ++m)
                        {
                            var alfa = (-2) * Math.PI * ((m * u) / height + (n * v) / width);
                            sumReal += grayImage[n][m] * Math.Cos(alfa);
                            sumImag += grayImage[n][m] * Math.Sin(alfa); //nie wiem czy kolejnosc dobra
                        }
                    }
                    var realPixelVal = reciprocalSquareOfWidthHeightImage * sumReal;
                    var imaginaryPixelVal = reciprocalSquareOfWidthHeightImage * sumImag;
                    int pixelValue = (int)Math.Round(Math.Sqrt(realPixelVal * realPixelVal + imaginaryPixelVal * imaginaryPixelVal));

                    coloredImage[v][column] = pixelValue;
                    coloredImage[v][++column] = pixelValue;
                    coloredImage[v][++column] = pixelValue;

                    maxModulValue = Math.Max(maxModulValue, grayImage[v][u]);
                }
            }
            for (var v = 0; v < height; ++v) //normalizacja - rozciagniecie na 0-255
            {
                var column = 0;
                for (var u = 0; u < width; ++u)
                {
                    var pixel = grayImage[v][u];
                    int newPixel = (int)Math.Round((double)(pixel * 255 / maxModulValue));
                    coloredImage[v][column] = newPixel;
                    coloredImage[v][++column] = newPixel;
                    coloredImage[v][++column] = newPixel;
                }
            }

            CreateSpectrumImage(coloredImage);
        }
    }
}
