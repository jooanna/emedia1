using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace emedia1
{
    public class ParsedImage
    {
        public List<string> Comments { get; set; } = new List<string>();

        public List<ColorComponent> ColorComponents { get; set; } = new List<ColorComponent>();

        public ImageType ImageType { get; set; }

        public int ImageHeight { get; set; }

        public int ImageWidth { get; set; }

        public List<HuffmanTable> HuffmanTables { get; set; }

        public string Scan { get; set; }

        public int SamplePrecision { get; set; }

        public QuantisationTable LuminanceTable { get; set; }

        public QuantisationTable ChrominanceTable { get; set; }

        public List<List<int>> ImagePixels { get; set; }

        public string PathToImage { get; set; }
    }
}