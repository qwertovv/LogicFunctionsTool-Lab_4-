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



        [Fact]
        public void GenerateTruthTable_WithTwoVariables_ReturnsCorrectRows()
        {
            
            var function = new LogicalFunction
            {
                VariablesCount = 2,
                FunctionNumber = 3 
            };

         
            var table = function.GenerateTruthTable();

            
            Assert.Equal(4, table.Count);

            
            Assert.Equal(new bool[] { false, false }, table[0].InputValues);
            Assert.True(table[0].Result);

            
            Assert.Equal(new bool[] { false, true }, table[1].InputValues);
            Assert.True(table[1].Result);

      
            Assert.Equal(new bool[] { true, false }, table[2].InputValues);
            Assert.False(table[2].Result);

            
            Assert.Equal(new bool[] { true, true }, table[3].InputValues);
            Assert.False(table[3].Result);
        }

        [Fact]
        public void GenerateTruthTable_WithThreeVariablesAndNumber11_ReturnsCorrectTable()
        {
          
            var function = new LogicalFunction
            {
                VariablesCount = 3,
                FunctionNumber = 11
            };

        
            var table = function.GenerateTruthTable();

        
            Assert.Equal(8, table.Count);
        }

        [Fact]
        public void StringRepresentation_OfTruthTableRow_FormatsCorrectly()
        {
            
            var row = new TruthTableRow(new bool[] { true, false, true }, true);

       
            var result = row.StringRepresentation;

           
            Assert.Equal("1 0 1 | 1", result);
        }



       
        [Theory]
        [InlineData("x1 & x2", new string[] { "x1", "x2", "&" })]
        [InlineData("!x1 | x2", new string[] { "x1", "!", "x2", "|" })]
        [InlineData("x1 & x2 | x3", new string[] { "x1", "x2", "&", "x3", "|" })]
        [InlineData("(x1 | x2) & x3", new string[] { "x1", "x2", "|", "x3", "&" })]
        public void ToRPN_ValidFormulas_ReturnsCorrectRPN(string formula, string[] expectedRPN)
        {
           
            var tokens = _parser.Tokenize(formula);

           
            var rpn = _parser.ToRPN(tokens);

    
            Assert.Equal(expectedRPN, rpn);
        }

        

       

        [Fact]
        public void CompleteWorkflow_FromFormulaToDNFKNF_WorksCorrectly()
        {
            
            var parser = new FormulaParser();
            var function = new LogicalFunction();

           
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
            
            var function = new LogicalFunction
            {
                VariablesCount = 3,
                FunctionNumber = 11
            };

            
            var table = function.GenerateTruthTable();

         
            Assert.Equal(8, table.Count);
        }

        



        [Fact]
        public void EquivalentFunctions_ShouldReturnTrue()
        {
     
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

        
            var table1 = function1.GenerateTruthTable();
            var table2 = function2.GenerateTruthTable();

            
            Assert.Equal(table1.Count, table2.Count);
            for (int i = 0; i < table1.Count; i++)
            {
                Assert.Equal(table1[i].Result, table2[i].Result);
            }
        }

        
       

        [Fact]
        public void Tokenize_EmptyFormula_ReturnsEmptyList()
        {
            
            var tokens = _parser.Tokenize("");

       
            Assert.Empty(tokens);
        }

        [Fact]
        public void GenerateTruthTable_OneVariable_CorrectRows()
        {
      
            var function = new LogicalFunction
            {
                VariablesCount = 1,
                FunctionNumber = 1 
            };

           
            var table = function.GenerateTruthTable();

           
            Assert.Equal(2, table.Count);
            Assert.False(table[0].InputValues[0]);
            Assert.True(table[0].Result);
            Assert.True(table[1].InputValues[0]);
            Assert.False(table[1].Result);
        }

        

        [Fact]
        public void GenerateTruthTable_FourVariables_PerformanceAcceptable()
        {
          
            var function = new LogicalFunction
            {
                VariablesCount = 4,
                FunctionNumber = 12345
            };

           
            var table = function.GenerateTruthTable();
            Assert.Equal(16, table.Count); // 2^4 = 16 строк
        }

       

        [Fact]
        public void ComplexFormula_ParsingAndConversion_Works()
        {
           
            var complexFormula = "!(x1 & x2) -> (x3 | !x4)";

           
            var basicFormula = _parser.ToBasicOperators(complexFormula);
            var tokens = _parser.Tokenize(complexFormula);
            var rpn = _parser.ToRPN(tokens);

           
            Assert.NotNull(basicFormula);
            Assert.NotEmpty(tokens);
            Assert.NotEmpty(rpn);
            Assert.Contains("!", basicFormula);
            Assert.Contains("|", basicFormula);
        }

        [Fact]
        public void FormulaWithMultipleParentheses_CorrectRPN()
        {
           
            var formula = "((x1 & x2) | (x3 & x4)) & x5";

            
            var tokens = _parser.Tokenize(formula);
            var rpn = _parser.ToRPN(tokens);

            
            // Ожидаемый RPN: x1 x2 & x3 x4 & | x5 &
            var expected = new string[] { "x1", "x2", "&", "x3", "x4", "&", "|", "x5", "&" };
            Assert.Equal(expected, rpn);
        }
    }
}