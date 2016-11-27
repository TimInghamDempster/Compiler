using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	class CodeGenerator
	{
		List<int> m_codeStream = new List<int>();

		public void GenerateCode(SyntaxNode abstractSyntaxTree, string storeDirectory)
		{
			m_codeStream = new List<int>();
			
			GenerateExpression(abstractSyntaxTree.m_children[0]);

			AddPostScript();
			WriteForBlockDevice(storeDirectory);
		}

		void GenerateExpression(SyntaxNode  expressionNode)
		{
			GeneratePrimitive(expressionNode.m_children[0], 0, true);

			for(int i = 1; i < expressionNode.m_children.Count; i += 2)
			{
				GeneratePrimitive(expressionNode.m_children[i + 1], 1, false);
				GenerateBinaryOperator(expressionNode.m_children[i], 0, 1);
			}
		}

		void GenerateBinaryOperator(SyntaxNode operatorNode, int targetRegister, int sourceRegister)
		{
			int instruction = targetRegister << 8 | targetRegister;
			int argument = sourceRegister;

			switch(operatorNode.m_type)
			{
			case ASTType.BinaryPlus:
				instruction |= (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Add;
				break;
			case ASTType.BinaryMinus:
				instruction |= (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Subtract;
				break;
			case ASTType.BinaryMul:
				instruction |= (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Multiply;
				break;
			case ASTType.BinaryDiv:
				instruction |= (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Divide;
				break;
			}

			m_codeStream.Add(instruction);
			m_codeStream.Add(argument);
		}

		void GenerateUnaryOperator(SyntaxNode unaryNode, int targetRegister)
		{
			GeneratePrimitive(unaryNode.m_children[0], targetRegister, false);
		}

		void GeneratePrimitive(SyntaxNode primitiveNode, int targetRegister, bool firstPrimitiveInExpression)
		{
			
			if(primitiveNode.m_type == ASTType.Primitive)
			{
				int val;
				int.TryParse(primitiveNode.m_data, out val);

				int instruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.SetLiteral | targetRegister << 8;

				m_codeStream.Add(instruction);
				m_codeStream.Add(val);
			}
			else if(primitiveNode.m_type == ASTType.UnaryMinus)
			{
				if (primitiveNode.m_children[0].m_type == ASTType.Primitive)
				{
					GenerateUnaryOperator(primitiveNode, targetRegister);

					int negativeInstruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.MultiplyLiteral | targetRegister << 8 | targetRegister;
					m_codeStream.Add(negativeInstruction);
					m_codeStream.Add(-1);
				}
            }
			else
			{
                if (!firstPrimitiveInExpression)
                {
                    int pushInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.Push;
                    m_codeStream.Add(pushInstruction);
                    m_codeStream.Add(0);
                }

				GenerateExpression(primitiveNode);

                if (!firstPrimitiveInExpression)
                {
                    int copyInstruction = (int)Virtual_Machine.UnitCodes.ALU | (int)Virtual_Machine.ALUOperations.Copy | 1 << 8 | 0;
                    m_codeStream.Add(copyInstruction);
                    m_codeStream.Add(0);

                    int popInstruction = (int)Virtual_Machine.UnitCodes.Stack | (int)Virtual_Machine.StackOperations.Pop;
                    m_codeStream.Add(popInstruction);
                    m_codeStream.Add(0);
                }
			}
		}

		void AddPostScript()
		{
			List<int> postScript = new List<int>()
			{
				(int)Virtual_Machine.UnitCodes.Branch	|	(int)Virtual_Machine.BranchOperations.Break
			};

			m_codeStream = m_codeStream.Concat(postScript).ToList();
		}

		void WriteForBlockDevice(string path)
		{
			if (!System.IO.Directory.Exists(path))
			{
				System.IO.Directory.CreateDirectory(path);
			}

			System.IO.FileStream block = new System.IO.FileStream(path + "/0.block", System.IO.FileMode.Create);
			System.IO.BinaryWriter blockWriter = new System.IO.BinaryWriter(block);

			int blockSize = 4096;
			int blockCount = m_codeStream.Count / blockSize + 1; // Plus one due to rounding down
			blockWriter.Write(blockCount);

			for(int val = 1; val < blockSize; val++)
			{
				blockWriter.Write(0);
			}
			blockWriter.Close();
			
			for(int codeBlock = 0; codeBlock < blockCount; codeBlock++)
			{
				int physicalBlock = codeBlock + 1;
				block = new System.IO.FileStream(path + "/" + physicalBlock.ToString() + ".block", System.IO.FileMode.Create);
				blockWriter = new System.IO.BinaryWriter(block);

				for(int indexWithinBlock = 0; indexWithinBlock < blockSize; indexWithinBlock++)
				{
					int codeIndex = codeBlock * blockSize + indexWithinBlock;

					if (codeIndex < m_codeStream.Count)
					{
						blockWriter.Write(m_codeStream[codeIndex]);
					}
					else
					{
						blockWriter.Write(0);
					}
				}
				blockWriter.Close();
			}
		}
	}
}
