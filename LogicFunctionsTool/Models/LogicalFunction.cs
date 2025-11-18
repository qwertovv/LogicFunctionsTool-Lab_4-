using System;
using System.Collections.Generic;
using System.Linq;

namespace LogicFunctionsTool.Models
{
    public class LogicalFunction
    {
        public int VariablesCount { get; set; }
        public string Formula { get; set; }
        public int FunctionNumber { get; set; }
        public bool[] TruthTable { get; set; }

        public List<TruthTableRow> GenerateTruthTable()
        {
            var table = new List<TruthTableRow>();
            int rowCount = (int)Math.Pow(2, VariablesCount);

            for (int i = 0; i < rowCount; i++)
            {
                var values = new bool[VariablesCount];
                for (int j = 0; j < VariablesCount; j++)
                {
                    values[j] = ((i >> (VariablesCount - j - 1)) & 1) == 1;
                }

                bool result = CalculateForValues(values);
                table.Add(new TruthTableRow(values, result));
            }

            return table;
        }

        private bool CalculateForValues(bool[] values)
        {
            // Простая реализация - потом заменим на парсер формул
            if (!string.IsNullOrEmpty(Formula))
            {
                return EvaluateFormula(Formula, values);
            }
            else
            {
                // По номеру функции
                int index = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i])
                        index |= (1 << (values.Length - i - 1));
                }
                return ((FunctionNumber >> index) & 1) == 1;
            }
        }

        private bool EvaluateFormula(string formula, bool[] values)
        {
            // Временная заглушка
            return true;
        }
    }

    public class TruthTableRow
    {
        public bool[] InputValues { get; set; }
        public bool Result { get; set; }

        public TruthTableRow(bool[] inputs, bool result)
        {
            InputValues = inputs;
            Result = result;
        }

        public string StringRepresentation
        {
            get
            {
                string inputs = string.Join(" ", InputValues.Select(v => v ? "1" : "0"));
                return $"{inputs} | {(Result ? "1" : "0")}";
            }
        }
    }

    public class NormalForm
    {
        public string Expression { get; set; }
        public int LiteralsCount { get; set; }
        public int ConjunctionsCount { get; set; }
        public int DisjunctionsCount { get; set; }
    }
}