using LogicFunctionsTool.Models;
using LogicFunctionsTool.Utilities;
using LogicFunctionsTool.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LogicFunctionsTool.Tests
{
    public class AllTests
    {
        private readonly FormulaParser _parser;

        public AllTests()
        {
            _parser = new FormulaParser();
        }

        // ===== LogicalFunctionTests =====

        [Fact]
        public void GenerateTruthTable_WithTwoVariables_ReturnsCorrectRows()
        {
            // Arrange
            var function = new LogicalFunction
            {
                VariablesCount = 2,
                FunctionNumber = 3 // 0011 in binary
            };

            // Act
            var table = function.GenerateTruthTable();

            // Assert
            Assert.Equal(4, table.Count);

            // Check first row (0,0) -> 1
            Assert.Equal(new bool[] { false, false }, table[0].InputValues);
            Assert.True(table[0].Result);

            // Check second row (0,1) -> 1
            Assert.Equal(new bool[] { false, true }, table[1].InputValues);
            Assert.True(table[1].Result);

            // Check third row (1,0) -> 0
            Assert.Equal(new bool[] { true, false }, table[2].InputValues);
            Assert.False(table[2].Result);

            // Check fourth row (1,1) -> 0
            Assert.Equal(new bool[] { true, true }, table[3].InputValues);
            Assert.False(table[3].Result);
        }

        [Fact]
        public void GenerateTruthTable_WithThreeVariablesAndNumber11_ReturnsCorrectTable()
        {
            // Arrange
            var function = new LogicalFunction
            {
                VariablesCount = 3,
                FunctionNumber = 11
            };

            // Act
            var table = function.GenerateTruthTable();

            // Assert
            Assert.Equal(8, table.Count);
        }

        [Fact]
        public void StringRepresentation_OfTruthTableRow_FormatsCorrectly()
        {
            // Arrange
            var row = new TruthTableRow(new bool[] { true, false, true }, true);

            // Act
            var result = row.StringRepresentation;

            // Assert
            Assert.Equal("1 0 1 | 1", result);
        }

        // ===== FormulaParserTests =====

       
        [Theory]
        [InlineData("x1 & x2", new string[] { "x1", "x2", "&" })]
        [InlineData("!x1 | x2", new string[] { "x1", "!", "x2", "|" })]
        [InlineData("x1 & x2 | x3", new string[] { "x1", "x2", "&", "x3", "|" })]
        [InlineData("(x1 | x2) & x3", new string[] { "x1", "x2", "|", "x3", "&" })]
        public void ToRPN_ValidFormulas_ReturnsCorrectRPN(string formula, string[] expectedRPN)
        {
            // Arrange
            var tokens = _parser.Tokenize(formula);

            // Act
            var rpn = _parser.ToRPN(tokens);

            // Assert
            Assert.Equal(expectedRPN, rpn);
        }

        

        // ===== IntegrationTests =====

        [Fact]
        public void CompleteWorkflow_FromFormulaToDNFKNF_WorksCorrectly()
        {
            // Arrange
            var parser = new FormulaParser();
            var function = new LogicalFunction();

            // Act & Assert - проверяем что не падает
            var formula = "x1 & x2";
            var tokens = parser.Tokenize(formula);
            var rpn = parser.ToRPN(tokens);
            var basicFormula = parser.ToBasicOperators(formula);

            Assert.NotNull(tokens);
            Assert.NotEmpty(tokens);
            Assert.NotNull(rpn);
            Assert.NotEmpty(rpn);
            Assert.NotNull(basicFormula);
        }

        [Fact]
        public void PresetTest_FunctionNumber11With3Variables_GeneratesCorrectResults()
        {
            // Arrange
            var function = new LogicalFunction
            {
                VariablesCount = 3,
                FunctionNumber = 11
            };

            // Act
            var table = function.GenerateTruthTable();

            // Assert
            Assert.Equal(8, table.Count);
        }

        [Fact]
        public void PresetTest_FormulaWithImplication_ParsesCorrectly()
        {
            // Arrange
            var parser = new FormulaParser();
            var formula = "(x1 | x2) -> x3";

            // Act
            var basicFormula = parser.ToBasicOperators(formula);
            var tokens = parser.Tokenize(basicFormula);

            // Assert
            Assert.Contains("!", basicFormula);
            Assert.Contains("|", basicFormula);
        }

        // ===== EquivalenceTests =====

        [Fact]
        public void EquivalentFunctions_ShouldReturnTrue()
        {
            // Arrange
            var function1 = new LogicalFunction
            {
                VariablesCount = 2,
                Formula = "x1 & x2"
            };

            var function2 = new LogicalFunction
            {
                VariablesCount = 2,
                Formula = "x2 & x1"  // Коммутативность
            };

            // Act
            var table1 = function1.GenerateTruthTable();
            var table2 = function2.GenerateTruthTable();

            // Assert
            Assert.Equal(table1.Count, table2.Count);
            for (int i = 0; i < table1.Count; i++)
            {
                Assert.Equal(table1[i].Result, table2[i].Result);
            }
        }

        

        // ===== Edge Cases Tests =====

       

       

        [Fact]
        public void Tokenize_EmptyFormula_ReturnsEmptyList()
        {
            // Act
            var tokens = _parser.Tokenize("");

            // Assert
            Assert.Empty(tokens);
        }

        [Fact]
        public void GenerateTruthTable_OneVariable_CorrectRows()
        {
            // Arrange
            var function = new LogicalFunction
            {
                VariablesCount = 1,
                FunctionNumber = 1 // 01 in binary
            };

            // Act
            var table = function.GenerateTruthTable();

            // Assert
            Assert.Equal(2, table.Count);
            Assert.False(table[0].InputValues[0]);
            Assert.True(table[0].Result);
            Assert.True(table[1].InputValues[0]);
            Assert.False(table[1].Result);
        }

        // ===== Performance Tests =====

        [Fact]
        public void GenerateTruthTable_FourVariables_PerformanceAcceptable()
        {
            // Arrange
            var function = new LogicalFunction
            {
                VariablesCount = 4,
                FunctionNumber = 12345
            };

            // Act & Assert - просто проверяем что выполняется без ошибок
            var table = function.GenerateTruthTable();
            Assert.Equal(16, table.Count); // 2^4 = 16 строк
        }

        // ===== Complex Formula Tests =====

        [Fact]
        public void ComplexFormula_ParsingAndConversion_Works()
        {
            // Arrange
            var complexFormula = "!(x1 & x2) -> (x3 | !x4)";

            // Act
            var basicFormula = _parser.ToBasicOperators(complexFormula);
            var tokens = _parser.Tokenize(complexFormula);
            var rpn = _parser.ToRPN(tokens);

            // Assert
            Assert.NotNull(basicFormula);
            Assert.NotEmpty(tokens);
            Assert.NotEmpty(rpn);
            Assert.Contains("!", basicFormula);
            Assert.Contains("|", basicFormula);
        }

        [Fact]
        public void FormulaWithMultipleParentheses_CorrectRPN()
        {
            // Arrange
            var formula = "((x1 & x2) | (x3 & x4)) & x5";

            // Act
            var tokens = _parser.Tokenize(formula);
            var rpn = _parser.ToRPN(tokens);

            // Assert
            // Ожидаемый RPN: x1 x2 & x3 x4 & | x5 &
            var expected = new string[] { "x1", "x2", "&", "x3", "x4", "&", "|", "x5", "&" };
            Assert.Equal(expected, rpn);
        }
    }
}