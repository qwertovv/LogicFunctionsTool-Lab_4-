
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LogicFunctionsTool.Utilities
{
    public class FormulaParser
    {
        private Dictionary<string, int> operatorsPriority = new Dictionary<string, int>
        {
            { "!", 5 }, { "not", 5 },
            { "&", 4 }, { "and", 4 },
            { "|", 3 }, { "or", 3 },
            { "^", 2 }, { "xor", 2 },
            { "->", 1 }, { "=", 0 }
        };

        public List<string> Tokenize(string formula)
        {
            var tokens = new List<string>();
            string current = "";

            foreach (char c in formula)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        tokens.Add(current);
                        current = "";
                    }
                    continue;
                }

                if (IsOperatorChar(c) || c == '(' || c == ')')
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        tokens.Add(current);
                        current = "";
                    }
                    tokens.Add(c.ToString());
                }
                else
                {
                    current += c;
                }
            }

            if (!string.IsNullOrEmpty(current))
                tokens.Add(current);

            return tokens;
        }

        public List<string> ToRPN(List<string> tokens)
        {
            var output = new List<string>();
            var stack = new Stack<string>();

            foreach (string token in tokens)
            {
                if (IsVariable(token))
                {
                    output.Add(token);
                }
                else if (token == "(")
                {
                    stack.Push(token);
                }
                else if (token == ")")
                {
                    while (stack.Count > 0 && stack.Peek() != "(")
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Pop();
                }
                else if (IsOperator(token))
                {
                    while (stack.Count > 0 && IsOperator(stack.Peek()) &&
                           operatorsPriority[stack.Peek()] >= operatorsPriority[token])
                    {
                        output.Add(stack.Pop());
                    }
                    stack.Push(token);
                }
            }

            while (stack.Count > 0)
            {
                output.Add(stack.Pop());
            }

            return output;
        }

        public string ToBasicOperators(string formula)
        {
            if (string.IsNullOrEmpty(formula))
                return formula;

            string result = formula;

            // Заменяем текстовые операторы на символьные
            result = result.Replace("not", "!")
                          .Replace("and", "&")
                          .Replace("or", "|")
                          .Replace("xor", "^");

            // Обрабатываем импликацию: A -> B ≡ !A | B
            var implicationPattern = @"([^>!\s&|^()]+)\s*->\s*([^>!\s&|^()]+)";
            result = Regex.Replace(result, implicationPattern, "!($1)|($2)");

            // Обрабатываем эквивалентность: A = B ≡ (A & B) | (!A & !B)
            var equivalencePattern = @"([^=!\s&|^()]+)\s*=\s*([^=!\s&|^()]+)";
            result = Regex.Replace(result, equivalencePattern, "($1&$2)|(!$1&!$2)");

            // Обрабатываем XOR: A ^ B ≡ (A & !B) | (!A & B)
            var xorPattern = @"([^^!\s&|()]+)\s*\^\s*([^^!\s&|()]+)";
            result = Regex.Replace(result, xorPattern, "($1&!$2)|(!$1&$2)");

            return result;
        }

        private bool IsOperatorChar(char c)
        {
            return c == '!' || c == '&' || c == '|' || c == '^' || c == '-' || c == '=';
        }

        private bool IsVariable(string token)
        {
            return (token.StartsWith("x") && token.Length > 1 && char.IsDigit(token[1])) ||
                   token == "0" || token == "1";
        }

        private bool IsOperator(string token)
        {
            return operatorsPriority.ContainsKey(token.ToLower());
        }
    }
}