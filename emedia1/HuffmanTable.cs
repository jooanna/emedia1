using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace emedia1
{
    public class HuffmanTable
    {
        private int id;
        private HuffmanTableType type;
        private List<List<string>> matrix;
        private int numberOfCodes;
        private static readonly int NUMBER_OF_BITS = 16;

        public HuffmanTable(string tableToParse)
        {
            //  00 01 00 01 05 01 01 00 00 00 00 00 00 00 00 00 00 00 04 02 03 05 06 07 01 08 

            // 0 class
            // 0 id

            // number of codes of length i (2 words)

            // value associated with each code

            // 01 00 01 05 01 01 00 00 00 00 00 00 00 00 00 00 00 04 02 03 05 06 07 01 08

            /*
                    Codes of length 01 bits (001 total): 00 
                    Codes of length 02 bits (000 total): 
                    Codes of length 03 bits (001 total): 04 
                    Codes of length 04 bits (005 total): 02 03 05 06 07 
                    Codes of length 05 bits (001 total): 01 
                    Codes of length 06 bits (001 total): 08 
                    Codes of length 07 bits (000 total): 
                    Codes of length 08 bits (000 total): 
                    Codes of length 09 bits (000 total): 
                    Codes of length 10 bits (000 total): 
                    Codes of length 11 bits (000 total): 
                    Codes of length 12 bits (000 total): 
                    Codes of length 13 bits (000 total): 
                    Codes of length 14 bits (000 total): 
                    Codes of length 15 bits (000 total): 
                    Codes of length 16 bits (000 total): 
            */
            int isClassAC = int.Parse(tableToParse.Substring(0, 1), NumberStyles.HexNumber);
            this.type = isClassAC == 1 ? HuffmanTableType.AC : HuffmanTableType.DC;

            this.id = int.Parse(tableToParse.Substring(1, 1), NumberStyles.HexNumber);

            List<int> numberOfCodesList = new List<int>();

            for (int i = Markers.WORD_LENGTH; i < NUMBER_OF_BITS*2 + Markers.WORD_LENGTH; i += Markers.WORD_LENGTH)
            {
                numberOfCodesList.Add(int.Parse(tableToParse.Substring(i, Markers.WORD_LENGTH), NumberStyles.HexNumber));
            }

            matrix = new List<List<string>>();
            for (int i = 0; i < NUMBER_OF_BITS; ++i)
            {
                matrix.Add(new List<string>());
            }

            int counter = NUMBER_OF_BITS*Markers.WORD_LENGTH + Markers.WORD_LENGTH;
            for (int n = 0; n < numberOfCodesList.Count; ++n)
            {
                numberOfCodes += numberOfCodesList[n];
                for (int i = 0; i < numberOfCodesList[n]*2; i += Markers.WORD_LENGTH)
                {
                    //parse to int????
                    matrix[n].Add(tableToParse.Substring(counter, Markers.WORD_LENGTH));
                    counter += Markers.WORD_LENGTH;
                }
            }
        }


        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Tablica Huffmana Id: ")
                .Append(this.Id)
                .Append(" ")
                .Append(this.Type1.ToString())
                .Append("  Ilosc kodow: ")
                .Append(numberOfCodes)
                .Append(Environment.NewLine);
            for (int i = 0 ; i < matrix.Count; ++i)
            {
                stringBuilder.Append("   Kody o ilosci bitów : ")
                    .Append(i)
                    .Append("(")
                    .Append(matrix[i].Count)
                    .Append(")")
                    .Append(": ");
                foreach (var code in matrix[i])
                {
                    stringBuilder.Append(code)
                        .Append(" ");
                }
                stringBuilder.Append(Environment.NewLine);
            }

            return stringBuilder.ToString();
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public List<List<string>> Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        public int NumberOfCodes
        {
            get { return numberOfCodes; }
            set { numberOfCodes = value; }
        }

        public HuffmanTableType Type1
        {
            get { return type; }
            set { type = value; }
        }
    }
}