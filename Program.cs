using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	class Program
	{
		static CodeGenerator m_codeGenerator = new CodeGenerator();
		static Lexer m_lexer = new Lexer();
		static Parser m_parser = new Parser();

		static void Main(string[] args)
		{
			string code = "9 + 3*5+(7/(2+1)) * -2";

			List<Token> tokenStream = m_lexer.Lex(code);

			SyntaxNode abstractSyntaxTree = new SyntaxNode();
			bool parsed = m_parser.Parse(tokenStream, abstractSyntaxTree);

			m_codeGenerator.GenerateCode(abstractSyntaxTree, @"MainDrive");

			System.Diagnostics.Process proc = System.Diagnostics.Process.Start(@"..\..\..\..\Virutal Machine\bin\Release\Virutal Machine.exe");
		}
	}
}
