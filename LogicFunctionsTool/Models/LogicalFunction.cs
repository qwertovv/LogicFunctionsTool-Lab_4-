
using System;
using System.Collections.Generic;
using System.Linq;
using LogicFunctionsTool.Utilities;

namespace LogicFunctionsTool.Models
{
    public class LogicalFunction
    {
        private FormulaParser parser = new FormulaParser();
        private List<string> rpnTokens;

        public int VariablesCount { get; set; }
        public string Formula { get; set; }
        public int FunctionNumber { get; set; }
        public bool[] TruthTable { get; set; }

        public List<TruthTableRow> GenerateTruthTable()
        {
            var table = new List<TruthTableRow>();
            int rowCount = (int)Math.Pow(2, VariablesCount);

            // Если задана формула, преобразуем ее в ОПЗ
            if (!string.IsNullOrEmpty(Formula))
            {
                var tokens = parser.Tokenize(Formula);
                rpnTokens = parser.ToRPN(tokens);
            }

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
            if (!string.IsNullOrEmpty(Formula) && rpnTokens != null)
            {
                return EvaluateRPN(values);
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

        private bool EvaluateRPN(bool[] values)
        {
            Stack<bool> stack = new Stack<bool>();

            foreach (string token in rpnTokens)
            {
                if (IsVariable(token))
                {
                    int varIndex = int.Parse(token.Substring(1)) - 1;
                    if (varIndex < values.Length)
                        stack.Push(values[varIndex]);
                    else
                        stack.Push(false);
                }
                else if (token == "!")
                {
                    bool operand = stack.Pop();
                    stack.Push(!operand);
                }
                else if (token == "&")
                {
                    bool right = stack.Pop();
                    bool left = stack.Pop();
                    stack.Push(left && right);
                }
                else if (token == "|")
                {
                    bool right = stack.Pop();
                    bool left = stack.Pop();
                    stack.Push(left || right);
                }
                else
                {
                    throw new InvalidOperationException($"Неизвестный оператор: {token}");
                }
            }

            return stack.Pop();
        }

        private bool IsVariable(string token)
        {
            return token.StartsWith("x") && token.Length > 1 && char.IsDigit(token[1]);
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