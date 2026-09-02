using System;
using System.Collections.Generic;
using System.Linq;
using AvatarGenerator.Core.Parameters;

namespace AvatarGenerator.Core.Dependencies
{
    public class ExpressionEvaluator : IExpressionEvaluator
    {
        public float Evaluate(string expression, IResolvedParameters context)
        {
            if (string.IsNullOrWhiteSpace(expression))
                return 0f;

            var tokens = Tokenize(expression);
            var ast = Parse(tokens);
            return EvaluateNode(ast, context);
        }

        public bool TryParse(string expression, out ExpressionNode ast)
        {
            try
            {
                var tokens = Tokenize(expression);
                ast = Parse(tokens);
                return true;
            }
            catch
            {
                ast = null;
                return false;
            }
        }

        public HashSet<string> GetDependencies(string expression)
        {
            var deps = new HashSet<string>();
            if (string.IsNullOrWhiteSpace(expression))
                return deps;

            var tokens = Tokenize(expression);
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Identifier)
                {
                    deps.Add(token.Value);
                }
            }
            return deps;
        }

        private List<Token> Tokenize(string expression)
        {
            var tokens = new List<Token>();
            int i = 0;

            while (i < expression.Length)
            {
                char c = expression[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                if (char.IsDigit(c) || c == '.')
                {
                    int start = i;
                    while (i < expression.Length && (char.IsDigit(expression[i]) || expression[i] == '.'))
                        i++;
                    tokens.Add(new Token(TokenType.Number, expression.Substring(start, i - start)));
                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    int start = i;
                    while (i < expression.Length && (char.IsLetterOrDigit(expression[i]) || expression[i] == '_' || expression[i] == '.'))
                        i++;
                    tokens.Add(new Token(TokenType.Identifier, expression.Substring(start, i - start)));
                    continue;
                }

                switch (c)
                {
                    case '+': tokens.Add(new Token(TokenType.Plus, "+")); i++; break;
                    case '-': tokens.Add(new Token(TokenType.Minus, "-")); i++; break;
                    case '*': tokens.Add(new Token(TokenType.Multiply, "*")); i++; break;
                    case '/': tokens.Add(new Token(TokenType.Divide, "/")); i++; break;
                    case '(': tokens.Add(new Token(TokenType.LParen, "(")); i++; break;
                    case ')': tokens.Add(new Token(TokenType.RParen, ")")); i++; break;
                    case ',': tokens.Add(new Token(TokenType.Comma, ",")); i++; break;
                    default: throw new ArgumentException($"Unknown character: {c}");
                }
            }

            tokens.Add(new Token(TokenType.End, ""));
            return tokens;
        }

        private ExpressionNode Parse(List<Token> tokens)
        {
            _tokens = tokens;
            _index = 0;
            return ParseExpression();
        }

        private List<Token> _tokens;
        private int _index;

        private Token Current => _tokens[_index];
        private Token Next => _tokens[_index + 1];

        private void Advance() => _index++;

        private ExpressionNode ParseExpression()
        {
            var left = ParseTerm();

            while (Current.Type == TokenType.Plus || Current.Type == TokenType.Minus)
            {
                var op = Current.Type;
                Advance();
                var right = ParseTerm();
                left = new BinaryOpNode(left, op, right);
            }

            return left;
        }

        private ExpressionNode ParseTerm()
        {
            var left = ParseFactor();

            while (Current.Type == TokenType.Multiply || Current.Type == TokenType.Divide)
            {
                var op = Current.Type;
                Advance();
                var right = ParseFactor();
                left = new BinaryOpNode(left, op, right);
            }

            return left;
        }

        private ExpressionNode ParseFactor()
        {
            if (Current.Type == TokenType.Number)
            {
                var value = float.Parse(Current.Value);
                Advance();
                return new NumberNode(value);
            }

            if (Current.Type == TokenType.Identifier)
            {
                var name = Current.Value;
                Advance();

                if (Current.Type == TokenType.LParen)
                {
                    Advance();
                    var args = new List<ExpressionNode>();
                    if (Current.Type != TokenType.RParen)
                    {
                        args.Add(ParseExpression());
                        while (Current.Type == TokenType.Comma)
                        {
                            Advance();
                            args.Add(ParseExpression());
                        }
                    }
                    if (Current.Type != TokenType.RParen)
                        throw new ArgumentException("Expected )");
                    Advance();
                    return new FunctionNode(name, args);
                }

                return new VariableNode(name);
            }

            if (Current.Type == TokenType.LParen)
            {
                Advance();
                var expr = ParseExpression();
                if (Current.Type != TokenType.RParen)
                    throw new ArgumentException("Expected )");
                Advance();
                return expr;
            }

            throw new ArgumentException($"Unexpected token: {Current.Type}");
        }

        private float EvaluateNode(ExpressionNode node, IResolvedParameters context)
        {
            switch (node)
            {
                case NumberNode n: return n.Value;
                case VariableNode v: return context.TryGetValue(v.Name, out var val) ? val : 0f;
                case BinaryOpNode b: return EvaluateBinary(b, context);
                case FunctionNode f: return EvaluateFunction(f, context);
                default: return 0f;
            }
        }

        private float EvaluateBinary(BinaryOpNode node, IResolvedParameters context)
        {
            var left = EvaluateNode(node.Left, context);
            var right = EvaluateNode(node.Right, context);

            switch (node.Operator)
            {
                case TokenType.Plus: return left + right;
                case TokenType.Minus: return left - right;
                case TokenType.Multiply: return left * right;
                case TokenType.Divide: return right != 0f ? left / right : 0f;
                default: return 0f;
            }
        }

        private float EvaluateFunction(FunctionNode node, IResolvedParameters context)
        {
            var args = node.Arguments.ConvertAll(a => EvaluateNode(a, context));

            switch (node.Name.ToLower())
            {
                case "min": return Mathf.Min(args[0], args[1]);
                case "max": return Mathf.Max(args[0], args[1]);
                case "clamp": return Mathf.Clamp(args[0], args[1], args[2]);
                case "lerp": return Mathf.Lerp(args[0], args[1], args[2]);
                case "abs": return Mathf.Abs(args[0]);
                case "sqrt": return Mathf.Sqrt(Mathf.Max(0f, args[0]));
                case "pow": return Mathf.Pow(args[0], args[1]);
                case "sin": return Mathf.Sin(args[0]);
                case "cos": return Mathf.Cos(args[0]);
                default: return 0f;
            }
        }

        private enum TokenType
        {
            Number,
            Identifier,
            Plus,
            Minus,
            Multiply,
            Divide,
            LParen,
            RParen,
            Comma,
            End
        }

        private struct Token
        {
            public TokenType Type;
            public string Value;

            public Token(TokenType type, string value)
            {
                Type = type;
                Value = value;
            }
        }

        private abstract class ExpressionNode { }

        private class NumberNode : ExpressionNode
        {
            public float Value;
            public NumberNode(float v) { Value = v; }
        }

        private class VariableNode : ExpressionNode
        {
            public string Name;
            public VariableNode(string n) { Name = n; }
        }

        private class BinaryOpNode : ExpressionNode
        {
            public ExpressionNode Left;
            public TokenType Operator;
            public ExpressionNode Right;
            public BinaryOpNode(ExpressionNode l, TokenType op, ExpressionNode r)
            {
                Left = l; Operator = op; Right = r;
            }
        }

        private class FunctionNode : ExpressionNode
        {
            public string Name;
            public List<ExpressionNode> Arguments;
            public FunctionNode(string n, List<ExpressionNode> args)
            {
                Name = n; Arguments = args;
            }
        }
    }
}