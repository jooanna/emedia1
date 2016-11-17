using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace emedia1
{
    public class ImageScan
    {

        private int numberOfComponents;
        private List<ColorComponent> colorComponents;
        private List<List<int>> dataBlocks;
        private string imageData;

        public ImageScan(string dataToParse, ParsedImage parsedImage)
        {
            colorComponents = new List<ColorComponent>();
            dataBlocks = new List<List<int>>();
            int startOfScanLength = int.Parse(dataToParse.Substring(0, Markers.WORD_LENGTH*2), NumberStyles.HexNumber);
            this.numberOfComponents = int.Parse(dataToParse.Substring(Markers.WORD_LENGTH * 2, Markers.WORD_LENGTH), NumberStyles.HexNumber);

            for (int i = 0; i < this.numberOfComponents; ++i)
            {
                var colorComponent = parseComponentId(dataToParse, parsedImage, i);
                colorComponents.Add(colorComponent);
            }

            this.imageData = dataToParse.Substring(startOfScanLength * Markers.WORD_LENGTH);
        }

        private void ParseDataBlocks()
        {
            int dc = 0;
        }


        private static ColorComponent parseComponentId(string dataToParse, ParsedImage parsedImage, int i)
        {
            int counter = Markers.WORD_LENGTH*3 + Markers.WORD_LENGTH*i*2;
            ColorComponent colorComponent = new ColorComponent();
            colorComponent.Id =
                int.Parse(dataToParse.Substring(counter, Markers.WORD_LENGTH));
            counter += Markers.WORD_LENGTH;
            int huffmanTableDCId = int.Parse(dataToParse.Substring(counter, 1));
            ++counter;
            int huffmanTableACId = int.Parse(dataToParse.Substring(counter, 1));
            ++counter;
            foreach (var huffmanTable in parsedImage.HuffmanTables)
            {
                if (HuffmanTableType.DC.Equals(huffmanTable.Type1) && huffmanTableDCId.Equals(huffmanTable.Id))
                {
                    colorComponent.dcHuffmanTable = huffmanTable;
                }
                if (HuffmanTableType.AC.Equals(huffmanTable.Type1) && huffmanTableACId.Equals(huffmanTable.Id))
                {
                    colorComponent.acHuffmanTable = huffmanTable;
                }
            }
            return colorComponent;
        }


        public List<List<int>> DataBlocks
        {
            get { return dataBlocks; }
            set { dataBlocks = value; }
        }

        public List<ColorComponent> ColorComponents
        {
            get { return colorComponents; }
            set { colorComponents = value; }
        }

        public int NumberOfComponents
        {
            get { return numberOfComponents; }
            set { numberOfComponents = value; }
        }

        public string ImageData
        {
            get { return imageData; }
            set { imageData = value; }
        }
    }
}