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
		Token m_nextToken;
		Token m_currentToken;
		int m_tokenIndex;

		public bool Parse(List<Token> tokenStream, SyntaxNode programNode)
		{
			m_tokenStream = tokenStream;
			// Assumes non-empty token stream
			m_currentToken = tokenStream[0];
			m_nextToken = tokenStream[1];

			bool expressionParsed = ParseExpression(programNode);

			if(expressionParsed && m_nextToken == null)
			{
				return true;
			}
			else
			{
				if(m_nextToken != null)
				{
					Console.WriteLine("Error: code after main expression");
				}
				return false;
			}
		}

		bool ParseExpression(SyntaxNode parent)
		{
			SyntaxNode expressionNode = new SyntaxNode();
			expressionNode.m_type = ASTType.Expression;
			
			if(ParsePrimitive(expressionNode))
			{
				while(m_nextToken != null)
				{
					if(!ParseBinaryOperator(expressionNode))
					{
						parent.m_children.Add(expressionNode);
						return true;
					}
					else
					{
						if(!ParsePrimitive(expressionNode))
						{
							Console.WriteLine("Error: binary operator with no second argument");
							return false;
						}
					}
				}
				parent.m_children.Add(expressionNode);
				return true;
			}
			return false;
		}

		void AdvanceToken()
		{
			if(m_tokenIndex + 1 < m_tokenStream.Count)
			{
				m_tokenIndex++;
				m_currentToken = m_nextToken;

				if(m_tokenIndex + 1 < m_tokenStream.Count)
				{
					m_nextToken = m_tokenStream[m_tokenIndex + 1];
				}
				else
				{
					m_nextToken = null;
				}
			}
		}

		bool ParsePrimitive(SyntaxNode parent)
		{
			SyntaxNode primitiveNode = new SyntaxNode();
			primitiveNode.m_type = ASTType.Primitive;

			if(m_currentToken.type == TokenType.Integer)
			{
				parent.m_children.Add(primitiveNode);
				primitiveNode.m_data = m_currentToken.data;
				AdvanceToken();
				return true;
			}
			else if(m_currentToken.type == TokenType.OpenPerenthesis)
			{
				AdvanceToken();
				if(ParseExpression(primitiveNode))
				{
					if(m_currentToken.type == TokenType.ClosePerenthesis)
					{
						AdvanceToken();
						parent.m_children.Add(primitiveNode);
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
				parent.m_children.Add(primitiveNode);
				return true;
			}
			else
			{
				return false;
			}
		}

		bool ParseUnaryOperator(SyntaxNode parent)
		{
			if(m_currentToken.type == TokenType.Minus)
			{
				SyntaxNode minusNode = new SyntaxNode();
				minusNode.m_type = ASTType.BinaryMinus;
				parent.m_children.Add(minusNode);
				AdvanceToken();
				return true;
			}
			return false;
		}

		bool ParseBinaryOperator(SyntaxNode parent)
		{
			SyntaxNode binaryNode = new SyntaxNode();

			switch(m_currentToken.type)
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
