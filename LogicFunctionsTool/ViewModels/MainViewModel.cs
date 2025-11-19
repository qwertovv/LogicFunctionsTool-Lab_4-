
using LogicFunctionsTool.Models;
using LogicFunctionsTool.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace LogicFunctionsTool.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private FormulaParser parser = new FormulaParser();

        // Вкладка "По номеру"
        public int VariablesCountByNumber { get; set; } = 2;
        public int FunctionNumber { get; set; } = 0;
        public List<TruthTableRow> TruthTableByNumber { get; set; }
        public string DNFByNumber { get; set; }
        public string KNFByNumber { get; set; }

        // Вкладка "По формуле"
        public string FormulaInput { get; set; } = "x1 & x2";
        public List<TruthTableRow> TruthTableByFormula { get; set; }
        public string DNFFormula { get; set; }
        public string KNFFormula { get; set; }

        // Вкладка "Сравнение"
        public string FirstFunction { get; set; } = "x1 & x2";
        public string SecondFunction { get; set; } = "x1 | x2";
        public string ComparisonResult { get; set; }
        public string CounterExample { get; set; }
        public List<TruthTableRow> FirstFunctionTruthTable { get; set; }
        public List<TruthTableRow> SecondFunctionTruthTable { get; set; }

        // Команды
        public ICommand GenerateByNumberCommand { get; }
        public ICommand ParseFormulaCommand { get; }
        public ICommand CompareFunctionsCommand { get; }

        public MainViewModel()
        {
            GenerateByNumberCommand = new RelayCommand(GenerateByNumber);
            ParseFormulaCommand = new RelayCommand(ParseFormula);
            CompareFunctionsCommand = new RelayCommand(CompareFunctions);
        }

        private void GenerateByNumber()
        {
            try
            {
                var function = new LogicalFunction
                {
                    VariablesCount = VariablesCountByNumber,
                    FunctionNumber = FunctionNumber
                };

                TruthTableByNumber = function.GenerateTruthTable();
                DNFByNumber = GenerateDNF(TruthTableByNumber);
                KNFByNumber = GenerateKNF(TruthTableByNumber);

                OnPropertyChanged(nameof(TruthTableByNumber));
                OnPropertyChanged(nameof(DNFByNumber));
                OnPropertyChanged(nameof(KNFByNumber));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void ParseFormula()
        {
            try
            {
                string basicFormula = parser.ToBasicOperators(FormulaInput);
                int varsCount = CountVariablesInFormula(FormulaInput);
                if (varsCount == 0) varsCount = 1;

                var function = new LogicalFunction
                {
                    VariablesCount = varsCount,
                    Formula = basicFormula
                };

                TruthTableByFormula = function.GenerateTruthTable();
                DNFFormula = GenerateDNF(TruthTableByFormula);
                KNFFormula = GenerateKNF(TruthTableByFormula);

                OnPropertyChanged(nameof(TruthTableByFormula));
                OnPropertyChanged(nameof(DNFFormula));
                OnPropertyChanged(nameof(KNFFormula));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void CompareFunctions()
        {
            try
            {
                string basicFormula1 = parser.ToBasicOperators(FirstFunction);
                string basicFormula2 = parser.ToBasicOperators(SecondFunction);

                int varsCount1 = CountVariablesInFormula(FirstFunction);
                int varsCount2 = CountVariablesInFormula(SecondFunction);
                int maxVars = Math.Max(varsCount1, varsCount2);
                if (maxVars == 0) maxVars = 1;

                var function1 = new LogicalFunction
                {
                    VariablesCount = maxVars,
                    Formula = basicFormula1
                };

                var function2 = new LogicalFunction
                {
                    VariablesCount = maxVars,
                    Formula = basicFormula2
                };

                // Генерируем таблицы истинности для обеих функций
                FirstFunctionTruthTable = function1.GenerateTruthTable();
                SecondFunctionTruthTable = function2.GenerateTruthTable();

                bool equivalent = true;
                string counterExample = "";

                // Сравниваем таблицы построчно
                for (int i = 0; i < FirstFunctionTruthTable.Count; i++)
                {
                    if (FirstFunctionTruthTable[i].Result != SecondFunctionTruthTable[i].Result)
                    {
                        equivalent = false;
                        counterExample = string.Join(" ", FirstFunctionTruthTable[i].InputValues.Select(v => v ? "1" : "0"));
                        break;
                    }
                }

                ComparisonResult = equivalent ? "Функции эквивалентны" : "Функции не эквивалентны";
                CounterExample = equivalent ? "Нет" : counterExample;

                // Обновляем привязки данных
                OnPropertyChanged(nameof(ComparisonResult));
                OnPropertyChanged(nameof(CounterExample));
                OnPropertyChanged(nameof(FirstFunctionTruthTable));
                OnPropertyChanged(nameof(SecondFunctionTruthTable));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка сравнения: {ex.Message}");
            }
        }

        internal string GenerateDNF(List<TruthTableRow> table)
        {
            var terms = new List<string>();
            foreach (var row in table.Where(r => r.Result))
            {
                var literals = new List<string>();
                for (int i = 0; i < row.InputValues.Length; i++)
                {
                    literals.Add(row.InputValues[i] ? $"x{i + 1}" : $"!x{i + 1}");
                }
                terms.Add($"({string.Join(" & ", literals)})");
            }
            return terms.Count > 0 ? string.Join(" | ", terms) : "0";
        }

        internal string GenerateKNF(List<TruthTableRow> table)
        {
            var terms = new List<string>();
            foreach (var row in table.Where(r => !r.Result))
            {
                var literals = new List<string>();
                for (int i = 0; i < row.InputValues.Length; i++)
                {
                    literals.Add(!row.InputValues[i] ? $"x{i + 1}" : $"!x{i + 1}");
                }
                terms.Add($"({string.Join(" | ", literals)})");
            }
            return terms.Count > 0 ? string.Join(" & ", terms) : "1";
        }

        internal int CountVariablesInFormula(string formula)
        {
            if (string.IsNullOrEmpty(formula))
                return 0;

            var variables = new HashSet<int>();
            var matches = System.Text.RegularExpressions.Regex.Matches(formula, @"x(\d+)");
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                if (match.Success && int.TryParse(match.Groups[1].Value, out int varNumber))
                {
                    variables.Add(varNumber);
                }
            }

            return variables.Count > 0 ? variables.Max() : 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute)
        {
            _execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}