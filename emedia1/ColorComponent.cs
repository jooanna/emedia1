using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace emedia1
{
    public class ColorComponent
    {
        public int Id { get; set; } //1 = Y, 2 = Cb, 3 = Cr
        public int VerticalSampleFactor { get; set; }
        public int HorizontalSampleFactor { get; set; }
        public int QuantizationTable { get; set; }
        public HuffmanTable dcHuffmanTable { get; set; }
        public HuffmanTable acHuffmanTable { get; set; }
    }
}