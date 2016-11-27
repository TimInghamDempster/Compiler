using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	enum ASTType
	{
		BinaryPlus,
		BinaryMinus,
		BinaryMul,
		BinaryDiv,
		UnaryMinus,
		Expression,
		Primitive,
	}

	class SyntaxNode
	{
		public List<SyntaxNode> m_children = new List<SyntaxNode>();
		public ASTType m_type;
		public string m_data;
	}

	class Parser
	{
		List<Token> m_tokenStream;
		int m_tokenIndex;

		public bool Parse(List<Token> tokenStream, SyntaxNode programNode)
		{
			m_tokenStream = tokenStream;

			bool expressionParsed = ParseExpression(programNode, 0);

			if(expressionParsed && m_tokenIndex + 1 == m_tokenStream.Count)
			{
				return true;
			}
			else
			{
                if (m_tokenStream[m_tokenIndex + 1] != null)
				{
					Console.WriteLine("Error: code after main expression");
				}
				return false;
			}
		}

		bool ParseExpression(SyntaxNode parent, int minimumPrecedence)
		{
			SyntaxNode expressionNode = new SyntaxNode();
			expressionNode.m_type = ASTType.Expression;
			
			if(ParsePrimitive(expressionNode))
			{
                while (GetPrecedence() >= minimumPrecedence)
                {
                    int nextPrecedence = GetPrecedence() + 1;
                    ParseBinaryOperator(expressionNode);

                    ParseExpression(expressionNode, nextPrecedence);
                }
			}
            if (expressionNode.m_children.Count > 1)
            {
                parent.m_children.Add(expressionNode);
            }
            else
            {
                parent.m_children.Add(expressionNode.m_children[0]);
            }
			return true;
		}

        int GetPrecedence()
        {
            switch (m_tokenStream[m_tokenIndex].type)
            {
                case TokenType.Plus:
                    {
                        return 1;
                    } break;
                case TokenType.Minus:
                    {
                        return 1;
                    } break;
                case TokenType.Multiply:
                    {
                        return 2;
                    } break;
                case TokenType.Divide:
                    {
                        return 2;
                    } break;
                default:
                    {
                        return -1;
                    } break;
            }
        }

		void AdvanceToken()
		{
            if (m_tokenIndex + 1 < m_tokenStream.Count)
            {
                m_tokenIndex++;
            }
		}

		bool ParsePrimitive(SyntaxNode parent)
		{
			SyntaxNode primitiveNode = new SyntaxNode();
			primitiveNode.m_type = ASTType.Primitive;

            if (m_tokenStream[m_tokenIndex].type == TokenType.Integer)
			{
				parent.m_children.Add(primitiveNode);
                primitiveNode.m_data = m_tokenStream[m_tokenIndex].data;
				AdvanceToken();
				return true;
			}
            else if (m_tokenStream[m_tokenIndex].type == TokenType.OpenPerenthesis)
			{
				AdvanceToken();
				if(ParseExpression(parent, 0))
				{
                    if (m_tokenStream[m_tokenIndex].type == TokenType.ClosePerenthesis)
					{
						AdvanceToken();
						//parent.m_children.Add(primitiveNode);
						return true;
					}
					else
					{
						Console.WriteLine("Error: missing closing parenthesis");
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else if(ParseUnaryOperator(primitiveNode))
			{
				parent.m_children.Add(primitiveNode.m_children[0]);
				return true;
			}
			else
			{
				return false;
			}
		}

		bool ParseUnaryOperator(SyntaxNode parent)
		{
            if (m_tokenStream[m_tokenIndex].type == TokenType.Minus)
			{
				SyntaxNode minusNode = new SyntaxNode();
				minusNode.m_type = ASTType.UnaryMinus;
				parent.m_children.Add(minusNode);
				AdvanceToken();
				
				if(!ParsePrimitive(minusNode))
				{
					Console.WriteLine("Error: Unary minus with no argument");
                    return false;
				}
				
				return true;
			}
			return false;
		}

		bool ParseBinaryOperator(SyntaxNode parent)
		{
			SyntaxNode binaryNode = new SyntaxNode();

            switch (m_tokenStream[m_tokenIndex].type)
			{
				case TokenType.Plus:
				{
					parent.m_children.Add(binaryNode);
					binaryNode.m_type = ASTType.BinaryPlus;
					AdvanceToken();
					return true;
				} break;
				case TokenType.Minus:
					{
						parent.m_children.Add(binaryNode);
						binaryNode.m_type = ASTType.BinaryMinus;
						AdvanceToken();
						return true;
					} break;
				case TokenType.Multiply:
					{
						parent.m_children.Add(binaryNode);
						binaryNode.m_type = ASTType.BinaryMul;
						AdvanceToken();
						return true;
					} break;
				case TokenType.Divide:
					{
						parent.m_children.Add(binaryNode);
						binaryNode.m_type = ASTType.BinaryDiv;
						AdvanceToken();
						return true;
					} break;				
				default:
				{
					return false;
				}break;
			}
		}
	}
}
