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
        public string SecondFunction { get; set; } = "!(x1 | x2)";
        public string ComparisonResult { get; set; }
        public string CounterExample { get; set; }

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
                var function = new LogicalFunction
                {
                    VariablesCount = CountVariablesInFormula(FormulaInput),
                    Formula = FormulaInput
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
                var function1 = new LogicalFunction
                {
                    VariablesCount = CountVariablesInFormula(FirstFunction),
                    Formula = FirstFunction
                };

                var function2 = new LogicalFunction
                {
                    VariablesCount = CountVariablesInFormula(SecondFunction),
                    Formula = SecondFunction
                };

                var table1 = function1.GenerateTruthTable();
                var table2 = function2.GenerateTruthTable();

                bool equivalent = true;
                string counterExample = "";

                for (int i = 0; i < table1.Count; i++)
                {
                    if (table1[i].Result != table2[i].Result)
                    {
                        equivalent = false;
                        counterExample = string.Join(" ", table1[i].InputValues.Select(v => v ? "1" : "0"));
                        break;
                    }
                }

                ComparisonResult = equivalent ? "Функции эквивалентны" : "Функции не эквивалентны";
                CounterExample = equivalent ? "Нет" : counterExample;

                OnPropertyChanged(nameof(ComparisonResult));
                OnPropertyChanged(nameof(CounterExample));
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка: {ex.Message}");
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

            var variables = new HashSet<string>();
            var tokens = formula.Split(new[] { ' ', '(', ')', '&', '|', '!', '^', '-', '>', '=', 'n', 'o', 't', 'a', 'd', 'r', 'x' },
                                      StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                if (token.StartsWith("x") && token.Length > 1 && char.IsDigit(token[1]))
                {
                    variables.Add(token);
                }
                else if (token.All(char.IsDigit) && token != "0" && token != "1")
                {
                    variables.Add($"x{token}");
                }
            }
            return variables.Count;
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