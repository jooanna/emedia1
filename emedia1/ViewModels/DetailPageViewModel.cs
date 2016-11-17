using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;

namespace emedia1.ViewModels
{
    public class DetailPageViewModel :ViewModelBase
    {
        private readonly INavigationService _navigationService;
        public string FilePath { get; set; } = @"c:\Users\Joanna\repositories\Emedia\emedia1\emedia1\emedia1\Assets\owl.jpg";
        public ImageParser ImageParser { get; set; }

        public DetailPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            Messenger.Default.Register<string>(this, filePath => FilePath = filePath);
            GetAttributes();
        }

        private async Task GetAttributes()
        {
            FileParser fileParser = new FileParser();
            byte[] data = await fileParser.GetResource(FilePath);
            var convertToHexa = fileParser.ConvertToHexa(data);
            ImageParser = new ImageParser(MarkerSplitter.splitMarkers(convertToHexa));
            ImageParser.ParsedImage1.PathToImage = FilePath;

            ParsedImage parsedImage = await ImageParser.ParseToImageInHexa();
            ImageParser.ParsedImage1 = parsedImage;
        }

        private string _attribute;

        public string Attribute
        {
            get { return _attribute; }
            set
            {
                if(_attribute == value) return;
                _attribute = value;
                RaisePropertyChanged();
            }
        }


        public ICommand GoToMainPage => new RelayCommand(() =>
        {
            _navigationService.NavigateTo("MainPage");
        });

        public ICommand ShowImageSize => new RelayCommand(OnShowImageSize);
        public ICommand ShowHuffmanTables => new RelayCommand(OnShowHuffmanTables);
        public ICommand ShowChrominance => new RelayCommand(OnShowChrominance);
        public ICommand ShowLuminance => new RelayCommand(OnShowLuminance);

        private void OnShowLuminance()
        {
            Attribute = ImageParser.ParsedImage1.LuminanceTable.ToString();
        }

        private void OnShowChrominance()
        {
            Attribute = ImageParser.ParsedImage1.ChrominanceTable.ToString();
        }


        private void OnShowHuffmanTables()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var parsedImage1HuffmanTable in ImageParser.ParsedImage1.HuffmanTables)
            {
                stringBuilder.Append(parsedImage1HuffmanTable);
                stringBuilder.Append(Environment.NewLine);
            }
            Attribute = stringBuilder.ToString();
        }

        private void OnShowImageSize()
        {
            Attribute = $"Height = {ImageParser.ParsedImage1.ImageHeight}   " +
                        $"Width = {ImageParser.ParsedImage1.ImageWidth} ";    
        }

    }
}
