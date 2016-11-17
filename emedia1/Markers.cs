namespace emedia1
{
    public class Markers
    {
        public static readonly string START_OF_IMAGE = "ffd8";
        public static readonly string JFIF_HEADER = "ffe0";
        public static readonly string DEFINE_QUANTIZATION_TABLE = "ffdb";
        public static readonly string DEFINE_HUFFMAN_TABLE = "ffc4";
        public static readonly string START_OF_FRAME = "ffc0";
        public static readonly string START_OF_SCAN = "ffda";
        public static readonly string COMMENT_MARKER = "fffe";
        public static readonly string END_OF_IMAGE = "ffd9";
        public static readonly int WORD_LENGTH = 2;
    }
}