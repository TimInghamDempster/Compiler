using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	class CodeGenerator
	{
		public void GenerateCode(SyntaxNode abstractSyntaxTree, string storeDirectory)
		{
			List<int> codeStream = new List<int>();
			
			AddPostScript(ref codeStream);
			WriteForBlockDevice(codeStream, storeDirectory);
		}

		void AddPostScript(ref List<int> codeStream)
		{
			List<int> postScript = new List<int>()
			{
				(int)Virtual_Machine.UnitCodes.Branch	|	(int)Virtual_Machine.BranchOperations.Break
			};

			codeStream = codeStream.Concat(postScript).ToList();
		}

		void WriteForBlockDevice(List<int> codeStream, string path)
		{
			if (!System.IO.Directory.Exists(path))
			{
				System.IO.Directory.CreateDirectory(path);
			}

			System.IO.FileStream block = new System.IO.FileStream(path + "/0.block", System.IO.FileMode.Create);
			System.IO.BinaryWriter blockWriter = new System.IO.BinaryWriter(block);

			int blockSize = 4096;
			int blockCount = codeStream.Count / blockSize + 1; // Plus one due to rounding down
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

					if(codeIndex < codeStream.Count)
					{
						blockWriter.Write(codeStream[codeIndex]);
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
