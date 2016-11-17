using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace emedia1
{
    public class ImageParser
    {
        private ParsedImage _parsedImage;
        private Dictionary<string, string> _splitMarkers;

        public ImageParser(Dictionary<string, string> splitMarkers)
        {
            this._splitMarkers = splitMarkers;
            this._parsedImage = new ParsedImage();
        }

        public Dictionary<string, string> SplitMarkers
        {
            get { return _splitMarkers; }
            set { _splitMarkers = value; }
        }

        public ParsedImage ParsedImage1
        {
            get { return _parsedImage; }
            set { _parsedImage = value; }
        }


        public async Task<ParsedImage> ParseToImageInHexa()
        {
            ValidateData(_splitMarkers);

            string value;
            if (_splitMarkers.TryGetValue(Markers.START_OF_FRAME, out value))
            {
                ParseFrame(value);
            }

            if (_splitMarkers.TryGetValue(Markers.DEFINE_QUANTIZATION_TABLE, out value))
            {
                ParseQuantizationTable(value);
            }

            if (_splitMarkers.TryGetValue(Markers.DEFINE_HUFFMAN_TABLE, out value))
            {
                ParseHuffmanTables(value);
            }

            if (_splitMarkers.TryGetValue(Markers.START_OF_SCAN, out value))
            {
                ParseScan(value);
            }
            if (_parsedImage.PathToImage != null)
            {
                //ilość wierszy, ilość kolumn
                _parsedImage.ImagePixels = new List<List<int>>();
                for (int i = 0; i < _parsedImage.ImageHeight; ++i)
                {
                    _parsedImage.ImagePixels.Add(new List<int>());
                }


                StorageFile storageFile =
                    await StorageFile.GetFileFromApplicationUriAsync(new Uri(_parsedImage.PathToImage));
                using (IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.Read))
                {
                    var bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);

                    WriteableBitmap writeableBitmap =
                        new WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                    fileStream.Seek(0);
                    await writeableBitmap.SetSourceAsync(fileStream);

                    using (writeableBitmap.GetBitmapContext())
                    {
                        for (var nrWiersza = 0; nrWiersza < _parsedImage.ImageHeight; ++nrWiersza)
                        {
                            for (var nrKolumny = 0; nrKolumny < _parsedImage.ImageWidth; ++nrKolumny)
                            {
                                var pixel = writeableBitmap.GetPixel(nrKolumny, nrWiersza);
                                _parsedImage.ImagePixels[nrWiersza].Add(pixel.R);
                                _parsedImage.ImagePixels[nrWiersza].Add(pixel.G);
                                _parsedImage.ImagePixels[nrWiersza].Add(pixel.B);
                            }
                        }

                    } // Invalidate and present in the Dispose call
                }
            }
            return _parsedImage;
        }

        private ImageScan ParseScan(string dataToPrse)
        {
            ImageScan imageScan = new ImageScan(dataToPrse.Substring(Markers.START_OF_SCAN.Length), _parsedImage);

            return imageScan;
        }


        private void ParseHuffmanTables(string huffmanTablesHex)
        {
            huffmanTablesHex += huffmanTablesHex + Markers.DEFINE_HUFFMAN_TABLE;
            /*
               ff c4 00 1c 00 01 00 01 05 01 01 00 00 00 00 00 00 00 00 00 00 00 04 02 03 05 06 07 01 08 
               ff c4 00 40 10 00 02 01 02 03 05 04 07 07 01 07 05 01 00 00 00 01 02 00 03 11 04 05 21 06 12 31 41 51 13 22 61 71 07 32 52 81 91 a1 c1 14 23 42 62 72 b1 d1 a2 33 63 73 82 92 b2 c2 08 15 25 44 e1 16 
               ff c4 00 1a 01 01 00 02 03 01 00 00 00 00 00 00 00 00 00 00 00 00 05 06 01 03 04 02 
               ff c4 00 35 11 00 02 01 03 02 04 03 07 03 04 02 03 00 00 00 00 00 01 02 03 04 11 05 21 12 31 41 51 13 61 71 22 81 91 b1 c1 d1 f0 32 a1 e1 06 14 33 f1 15 23 34 52 72 
               */
            _parsedImage.HuffmanTables = new List<HuffmanTable>();
            int startOfTableIndex = 0;
            int endOfTableIndex = 0;
            int tableLength = 0;
            for (int i = 0; i < 4; ++i)
            {
                if (endOfTableIndex >= 0)
                {
                    startOfTableIndex = huffmanTablesHex.IndexOf(Markers.DEFINE_HUFFMAN_TABLE, endOfTableIndex);
                }
                if (startOfTableIndex >= 0)
                {
                    endOfTableIndex = huffmanTablesHex.IndexOf(Markers.DEFINE_HUFFMAN_TABLE,
                        startOfTableIndex + Markers.DEFINE_HUFFMAN_TABLE.Length);
                }

                tableLength = endOfTableIndex - startOfTableIndex - 4 * Markers.WORD_LENGTH;
                if (tableLength > 0)
                {
                    _parsedImage.HuffmanTables.Add(
                        new HuffmanTable(huffmanTablesHex.Substring(startOfTableIndex + 4 * Markers.WORD_LENGTH,
                            tableLength)));
                }
            }
        } 
        private void ParseQuantizationTable(string quantizationTable)
        {
            /*
                                 ff db 
                                 00 84 
                                 00 09 06 07 13 12 10 15 10 12 13 10 10 10 10 18 0f 17 16 10 16 12 10 10 10 12 10 17 15 16 18 15 11 15 15 18 1d 28 20 18 1a 27 1b 15 15 22 31 21 25 29 2b 2e 2e 2e 17 1f 33 38 33 2d 37 28 2d 2e 2b 
                                 01 0a 0a 0a 0e 0d 0e 1b 10 10 1a 30 26 20 25 2b 2b 2d 2f 2e 2f 30 2d 2d 2f 2d 2d 2d 2e 2d 2d 2b 2d 2b 2d 2d 2d 2b 2d 2d 2d 2d 2d 2b 2d 2d 2d 2d 2d 2b 2d 2f 2f 2d 2d 2d 2d 2d 2d 2d 2d 2d 2d 2d 2d 
                                                            */

            int tableSize = (quantizationTable.Length - Markers.DEFINE_HUFFMAN_TABLE.Length - 2 * Markers.WORD_LENGTH) / 2;
            string luminanceTableHex = quantizationTable.Substring(Markers.DEFINE_HUFFMAN_TABLE.Length + 2 * Markers.WORD_LENGTH,
                tableSize);

            string chrominanceTableHex = quantizationTable.Substring(Markers.DEFINE_HUFFMAN_TABLE.Length + 2 * Markers.WORD_LENGTH + tableSize,
                tableSize);
            _parsedImage.LuminanceTable = new QuantisationTable(luminanceTableHex, "Tablica kwantyzacji dla luminancji");
            _parsedImage.ChrominanceTable = new QuantisationTable(chrominanceTableHex, "Tablica kwantyzacji dla chrominancji");
        }


        private void ParseFrame(string frame)
        {
            /*
            1.      Start of Frame length
            2.      Precision (Bits per pixel per color component)
            3.      Image height
            4.      Image width
            5.      Number of color components
            6.      For each component
                1.      An ID
                2.      A vertical sample factor
                3.      A horizontal sample factor
                4.      A quantization table

                    ff c0 00 11 08 00 db 00 e7 03 01 11 00 02 11 01 03 11 01 
            */

            if (frame.StartsWith(Markers.START_OF_FRAME) == false)
            {
                throw new Exception("Brak znacznika ramki " + Markers.START_OF_FRAME);
            }

            var precisionHex = frame.Substring(Markers.WORD_LENGTH * 4, Markers.WORD_LENGTH);
            _parsedImage.SamplePrecision = int.Parse(precisionHex, NumberStyles.HexNumber);

            var heightHex = frame.Substring(Markers.WORD_LENGTH * 5, Markers.WORD_LENGTH * 2);
            _parsedImage.ImageHeight = int.Parse(heightHex, NumberStyles.HexNumber);

            var widthHex = frame.Substring(Markers.WORD_LENGTH * 7, Markers.WORD_LENGTH * 2);
            _parsedImage.ImageWidth = int.Parse(widthHex, NumberStyles.HexNumber);

            var numberOfColorComponents = int.Parse(frame.Substring(Markers.WORD_LENGTH * 9, Markers.WORD_LENGTH), NumberStyles.HexNumber);

            _parsedImage.ImageType = numberOfColorComponents > 1 ? ImageType.RGB : ImageType.GRAYSCALE;

            int counter = Markers.WORD_LENGTH * 10;
            for (int i = 0; i < numberOfColorComponents; ++i)
            {
                ColorComponent colorComponent = new ColorComponent();
                colorComponent.Id = int.Parse(frame.Substring(counter, Markers.WORD_LENGTH), NumberStyles.HexNumber);
                counter += Markers.WORD_LENGTH;

                colorComponent.VerticalSampleFactor = int.Parse(frame.Substring(counter, Markers.WORD_LENGTH / 2), NumberStyles.HexNumber);
                counter += Markers.WORD_LENGTH / 2;
                colorComponent.HorizontalSampleFactor = int.Parse(frame.Substring(counter, Markers.WORD_LENGTH / 2), NumberStyles.HexNumber);
                counter += Markers.WORD_LENGTH / 2;
                colorComponent.QuantizationTable = int.Parse(frame.Substring(counter, Markers.WORD_LENGTH), NumberStyles.HexNumber);
                counter += Markers.WORD_LENGTH;

                _parsedImage.ColorComponents.Add(colorComponent);
            }
        }


        private void ValidateData(Dictionary<string, string> splitMarkers)
        {
            List<string> requiredMarkers = new List<string>
            {
                Markers.START_OF_IMAGE,
                Markers.DEFINE_HUFFMAN_TABLE,
                Markers.END_OF_IMAGE,
                Markers.DEFINE_QUANTIZATION_TABLE,
                Markers.JFIF_HEADER,
                //Markers.START_OF_FRAME,
                Markers.START_OF_SCAN
            };
            foreach (var marker in requiredMarkers)
            {
                validateRequiredElement(splitMarkers, marker);
            }
        }

        private static void validateRequiredElement(Dictionary<string, string> splitMarkers, string marker)
        {
            if (splitMarkers.ContainsKey(marker) == false)
            {
                throw new Exception("Dane nieprawidłowe - brak znacznika: " + marker);
            }
        }
    }
}