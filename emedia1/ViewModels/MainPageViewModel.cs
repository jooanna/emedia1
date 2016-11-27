using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
            _selectedFile = new Image { Source = new BitmapImage(new Uri("ms-appx:///Assets/lines.jpg")) };
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


        public string FilePath { get; set; } = @"ms-appx:///Assets/lines.jpg";

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


        public static byte[] ToGreyscale(byte[] srcPixels, int width, int height)
        {
            byte b, g, r, a, luminance;
            byte[] destPixels = new byte[4 * width * height];

            // Convert pixel data to greyscale
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    b = srcPixels[(x + y * width) * 4];
                    g = srcPixels[(x + y * width) * 4 + 1];
                    r = srcPixels[(x + y * width) * 4 + 2];
                    a = srcPixels[(x + y * width) * 4 + 3];
                    luminance = Convert.ToByte(0.299 * r + 0.587 * g + 0.114 * b);

                    destPixels[(x + y * width) * 4] = luminance;     // B
                    destPixels[(x + y * width) * 4 + 1] = luminance; // G
                    destPixels[(x + y * width) * 4 + 2] = luminance; // R
                    destPixels[(x + y * width) * 4 + 3] = luminance; // A

                }
            }

            return destPixels;
        }

        public async Task CreateSpectrumImage(int[][] spectrum)
        {

            byte[] bytes = new byte[4*50*50];

            WriteableBitmap wb = new WriteableBitmap(50, 50);
            
            using (Stream stream = wb.PixelBuffer.AsStream())
            {
                if (stream.CanWrite)
                {
                    int index = 0;
                    foreach (var listOfInts in spectrum)
                    {
                        foreach (var ints in listOfInts)
                        {
                            bytes[index++] = Convert.ToByte(ints);
                        }
                    }
                    var newBytes = ToGreyscale(bytes, 50, 50);
                    await stream.WriteAsync(bytes, 0, bytes.Length);
                    //await stream.WriteAsync(newBytes, 0, newBytes.Length);
                    stream.Flush();
                    TransformedFile.Source = wb;
                }
            }


 //           var bitMap = ConvertToPicture(array);
   //         TransformedFile.Source = await bitMap;
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
                for (var u = 0; u < width*3; u+=3)
                {
                    var pixelValue = greyTab[v][u] * (int)Math.Pow(-1, v + u);
                    coloredTab[v][u] = pixelValue;
                    coloredTab[v][u+1] = pixelValue;
                    coloredTab[v][u+2] = pixelValue;
                    //coloredTab[v][u+2] = greyTab[v][u+3];
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
                for (var c = 0; c < width*3; c+=3)
                {
                    var b = coloredImage[r][c];
                    var g = coloredImage[r][c + 1];
                    var red = coloredImage[r][c + 2];
                    var a = coloredImage[r][c + 3];
                    var luminance = (0.299 * red + 0.587 * g + 0.114 * b);
                    grayImage[r].Add((int)luminance);
                    grayImage[r].Add((int)luminance);
                    grayImage[r].Add((int)luminance);
                    grayImage[r].Add((int)luminance);
                }
            }
            var reciprocalSquareOfWidthHeightImage = 1 / (Math.Sqrt(width * height));
            FftShift(height, width, grayImage, coloredImage);
            var maxModulValue = 0;

            var convertedTab = new int[height][];
            for (var i = 0; i < height; ++i)
            {
                //convertedTab[i] = new int[width];
               convertedTab[i] = new int[width*4];
            }

            for (var v = 0; v < height; ++v) // y
            {
               // for (var u = 0; u < width; u+=1)
                for (var u = 0; u < width*4; u+=4) //x
                {
                    double sumReal = 0;
                    double sumImag = 0;
                    for (var n = 0; n < height; ++n)
                    {
                        //for (var m = 0; m < width; m+=1)
                        for (var m = 0; m < width*4; m+=4)
                        {
                            var alfa = (-2) * Math.PI * ((m * u) / width + (n * v) / height);
                            sumReal += grayImage[n][m] * Math.Cos(alfa);
                            sumImag += grayImage[n][m] * Math.Sin(alfa); //nie wiem czy kolejnosc dobra
                        }
                    }
                    var realPixelVal = reciprocalSquareOfWidthHeightImage * sumReal;
                    var imaginaryPixelVal = reciprocalSquareOfWidthHeightImage * sumImag;
                    int pixelValue = (int)Math.Round(Math.Sqrt(realPixelVal * realPixelVal + imaginaryPixelVal * imaginaryPixelVal));

                    convertedTab[v][u] = pixelValue;
                    convertedTab[v][u+1] = pixelValue;
                    convertedTab[v][u+2] = pixelValue;
                    convertedTab[v][u+3] = pixelValue;

                    maxModulValue = Math.Max(maxModulValue, convertedTab[v][u]);
                }
            }
            for (var v = 0; v < height; ++v) //normalizacja - rozciagniecie na 0-255
            {
                for (var u = 0; u < width*4; u+=4)
                {
                    var pixel = convertedTab[v][u];
                    int newPixel = (int)Math.Round((double)(pixel * 255 / maxModulValue));
                    convertedTab[v][u] = newPixel;
                    convertedTab[v][u+1] = newPixel;
                    convertedTab[v][u+2] = newPixel;
                    convertedTab[v][u+3] = newPixel;
                }
            }

            CreateSpectrumImage(convertedTab);
        }
    }
}
