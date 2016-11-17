using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace emedia1
{
    public class MarkerSplitter

    {
        public static readonly int WORD_LENGTH = 2;


        public static Dictionary<string, string> splitMarkers(string hexImage)
        {
            var splitMarkers = new Dictionary<string, string>();
            parseStartOfImage(hexImage, splitMarkers);
            parseJfifHeader(hexImage, splitMarkers);
            parseQuantizationTable(hexImage, splitMarkers);
            parseStartOfFrame(hexImage, splitMarkers);
            parseHuffmanTable(hexImage, splitMarkers);
            parseScan(hexImage, splitMarkers);
            parseEndOfImage(hexImage, splitMarkers);
            parseComments(hexImage, splitMarkers);
            return splitMarkers;
        }

        private static void parseComments(string hexImage, Dictionary<string, string> splitMarkers)
        {
            if (hexImage.Contains(Markers.COMMENT_MARKER))
            {
                var indexOfNextMarker = hexImage.Length;
                if (hexImage.Contains(Markers.END_OF_IMAGE))
                {
                    indexOfNextMarker = hexImage.IndexOf(Markers.END_OF_IMAGE);
                }

                var comments = hexImage.Substring(hexImage.IndexOf(Markers.COMMENT_MARKER),
                    indexOfNextMarker - hexImage.IndexOf(Markers.COMMENT_MARKER));

                splitMarkers.Add(Markers.COMMENT_MARKER, comments);
            }
        }

        private static void parseScan(string hexImage, Dictionary<string, string> splitMarkers)
        {
            if (hexImage.Contains(Markers.START_OF_SCAN))
            {
                var indexOfNextMarker = hexImage.Length;
                if (hexImage.Contains(Markers.COMMENT_MARKER))
                {
                    indexOfNextMarker = hexImage.IndexOf(Markers.COMMENT_MARKER);
                }
                else
                {
                    if (hexImage.Contains(Markers.END_OF_IMAGE))
                    {
                        indexOfNextMarker = hexImage.IndexOf(Markers.END_OF_IMAGE);
                    }
                }

                var scan = hexImage.Substring(hexImage.IndexOf(Markers.START_OF_SCAN),
                    indexOfNextMarker - hexImage.IndexOf(Markers.START_OF_SCAN));

                splitMarkers.Add(Markers.START_OF_SCAN, scan);
            }
        }

        private static void parseQuantizationTable(string hexImage, Dictionary<string, string> splitMarkers)
        {
            if (hexImage.Contains(Markers.DEFINE_QUANTIZATION_TABLE))
            {
                var headerLength = parseHeaderLength(hexImage, Markers.DEFINE_QUANTIZATION_TABLE);
                var quantizationTable = hexImage.Substring(hexImage.IndexOf(Markers.DEFINE_QUANTIZATION_TABLE),
                    Markers.DEFINE_QUANTIZATION_TABLE.Length + headerLength);

                splitMarkers.Add(Markers.DEFINE_QUANTIZATION_TABLE, quantizationTable);
            }
        }

        private static void parseHuffmanTable(string hexImage, Dictionary<string, string> splitMarkers)
        {
            if (hexImage.Contains(Markers.DEFINE_HUFFMAN_TABLE))
            {
                var headerLength = parseLastHeaderLength(hexImage, Markers.DEFINE_HUFFMAN_TABLE);
                var startOfFirstTable = hexImage.IndexOf(Markers.DEFINE_HUFFMAN_TABLE);
                var startOfLastTable = hexImage.LastIndexOf(Markers.DEFINE_HUFFMAN_TABLE);
                var lengthOfHeader = startOfLastTable + Markers.DEFINE_HUFFMAN_TABLE.Length + headerLength -
                                     startOfFirstTable;
                var huffmanTable = hexImage.Substring(startOfFirstTable, lengthOfHeader);

                splitMarkers.Add(Markers.DEFINE_HUFFMAN_TABLE, huffmanTable);
            }
        }

        private static void parseJfifHeader(string hexImage, Dictionary<string, string> splitMarkers)
        {
            if (hexImage.Contains(Markers.JFIF_HEADER))
            {
                var headerLength = parseHeaderLength(hexImage, Markers.JFIF_HEADER);
                var jfifHeader = hexImage.Substring(hexImage.IndexOf(Markers.JFIF_HEADER),
                    Markers.JFIF_HEADER.Length + headerLength);

                splitMarkers.Add(Markers.JFIF_HEADER, jfifHeader);
            }
        }

        private static void parseStartOfFrame(string hex, Dictionary<string, string> splitMarkers)
        {
            if (hex.Contains(Markers.START_OF_FRAME))
            {
                var frameLength = parseHeaderLength(hex, Markers.START_OF_FRAME);
                var startOfFrame = hex.Substring(hex.IndexOf(Markers.START_OF_FRAME),
                    Markers.START_OF_FRAME.Length + frameLength);

                splitMarkers.Add(Markers.START_OF_FRAME, startOfFrame);
            }
        }

        private static int parseHeaderLength(string hex, string headerMarker)
        {
            var frameLengthHex = hex.Substring(hex.IndexOf(headerMarker) + headerMarker.Length,
                WORD_LENGTH*2);
            var frameLength = int.Parse(frameLengthHex, NumberStyles.HexNumber);
            return frameLength*WORD_LENGTH;
        }

        private static int parseLastHeaderLength(string hex, string headerMarker)
        {
            var frameLengthHex = hex.Substring(hex.LastIndexOf(headerMarker) + headerMarker.Length,
                WORD_LENGTH*2);
            var frameLength = int.Parse(frameLengthHex, NumberStyles.HexNumber);
            return frameLength*WORD_LENGTH;
        }

        private static void parseEndOfImage(string hexImage, Dictionary<string, string> splitMarkers)
        {
            if (hexImage.EndsWith(Markers.END_OF_IMAGE))
                splitMarkers.Add(Markers.END_OF_IMAGE,
                    hexImage.Substring(hexImage.IndexOf(Markers.END_OF_IMAGE), Markers.END_OF_IMAGE.Length));
        }

        private static void parseStartOfImage(string hexImage, Dictionary<string, string> splitMarkers)
        {
            if (hexImage.StartsWith(Markers.START_OF_IMAGE))
                splitMarkers.Add(Markers.START_OF_IMAGE,
                    hexImage.Substring(hexImage.IndexOf(Markers.START_OF_IMAGE), Markers.START_OF_IMAGE.Length));
        }
    }
}