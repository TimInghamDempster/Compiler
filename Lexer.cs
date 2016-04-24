using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	public enum TokenType
	{
		Plus,
		Minus,
		Multiply,
		Divide,
		OpenPerenthesis,
		ClosePerenthesis,
		Integer,
	}

	class Token
	{
		public TokenType type;
		public char data;
	}

	class Lexer
	{
		public List<Token> Lex(string input)
		{
			List<Token> tokens = new List<Token>();

			for(int i = 0; i < input.Length; i++)
			{
				switch(input[i])
				{
					case '+':
						tokens.Add(new Token() {type = TokenType.Plus});
						break;
					case '-':
						tokens.Add(new Token() { type = TokenType.Minus });
						break;
					case '*':
						tokens.Add(new Token() { type = TokenType.Multiply });
						break;
					case '/':
						tokens.Add(new Token() { type = TokenType.Divide });
						break;
					case '(':
						tokens.Add(new Token() { type = TokenType.OpenPerenthesis });
						break;
					case ')':
						tokens.Add(new Token() { type = TokenType.ClosePerenthesis });
						break;
					default:
						int temp;
						if(int.TryParse(input[i].ToString(), out temp))
						{
							tokens.Add(new Token() { type = TokenType.Integer, data = input[i] });
						}
						break;
				}
			}

			return tokens;
		}	
	}
}
