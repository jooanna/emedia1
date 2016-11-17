using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace emedia1
{
    public class QuantisationTable
    {
        private int id;
        private List<List<int>> matrix;
        private List<int> quantistationVectorZigZapped;
        private readonly int PRECISION = 8;
        private string name;


        public QuantisationTable(string tableToParse, string name)
        {
            this.name = name;
            //   00 
            // 09 06 07 13 12 10 15 10 12 13 10 10 10 10 18 0f 17 16 10 16 12 10 10 10 12 10 17 15 16 18 15 11 15 15 18 1d 28 20 18 1a 27 1b 15 15 22 31 21 25 29 2b 2e 2e 2e 17 1f 33 38 33 2d 37 28 2d 2e 2b
            this.id = int.Parse(tableToParse.Substring(0, Markers.WORD_LENGTH), NumberStyles.HexNumber);
            List<int> quantistationVector = new List<int>();

            for (int i = Markers.WORD_LENGTH; i < tableToParse.Length; i += Markers.WORD_LENGTH)
            {
                quantistationVector.Add(int.Parse(tableToParse.Substring(i, Markers.WORD_LENGTH), NumberStyles.HexNumber));
            }
            this.quantistationVectorZigZapped = quantistationVector;
            matrix = new List<List<int>>();
            for (int i = 0; i < PRECISION; ++i)
            {
                matrix.Add(new List<int>());
            }
            List<List<int>> zigzapPattern = new List<List<int>>();
            zigzapPattern.Add(new List<int> {0, 1, 5, 6, 14, 15, 27, 28});
            zigzapPattern.Add(new List<int> {2, 4, 7, 13, 16, 26, 29, 42});
            zigzapPattern.Add(new List<int> {3, 8, 12, 17, 25, 30, 41, 43});
            zigzapPattern.Add(new List<int> {9, 11, 18, 24, 31, 40, 44, 53});
            zigzapPattern.Add(new List<int> {10, 19, 23, 32, 39, 45, 52, 54});
            zigzapPattern.Add(new List<int> {20, 22, 33, 38, 46, 51, 55, 60});
            zigzapPattern.Add(new List<int> {21, 34, 37, 47, 50, 56, 59, 61});
            zigzapPattern.Add(new List<int> {35, 36, 48, 49, 57, 58, 62, 63});

            for (int i = 0; i < zigzapPattern.Count; ++i)
            {
                for (int j = 0; j < zigzapPattern[i].Count; ++j)
                {
                    matrix[i].Add(quantistationVector[zigzapPattern[i][j]]);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(name)
                .Append(" ")
                .Append("Id :")
                .Append(this.Id)
                .Append("\n");
            foreach (var rows in Matrix)
            {
                foreach (var cell in rows)
                {
                    if (cell < 10)
                    {
                        builder.Append(" ");
                    }
                    builder.Append(cell).Append(" ");
                }
                builder.Append("\n");

            }
            builder.Append("\n");
            return builder.ToString();
        }


        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public List<List<int>> Matrix
        {
            get { return matrix; }
            set { matrix = value; }
        }

        public List<int> QuantistationVectorZigZapped
        {
            get { return quantistationVectorZigZapped; }
            set { quantistationVectorZigZapped = value; }
        }
    }
}