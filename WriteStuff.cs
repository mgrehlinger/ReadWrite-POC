using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ReadWrite
{
	internal class WriteStuff
	{
		string FileName { get; set; }
		int BlockSize { get; set; }
		CancellationTokenSource cancellationToken;
		internal WriteStuff(string fileName, int blockSize, CancellationTokenSource cancelToken)
		{
			FileName = fileName;
			BlockSize = blockSize;
			cancellationToken = cancelToken;
		}

		internal async Task WriteLoopAsync()
		{
			int blockCnt = 0;
			while (true)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					Console.WriteLine("Cancel WriteStuff");
					break;
				}
				await doWriteAsync(blockCnt++);
			}
		}

		async Task<bool> doWriteAsync(int blockId)
		{
			Debug.WriteLine("  ..Write block " + blockId);
			var payload = buildBlock();
			var md5 = MD5.Create();
			var hash = md5.ComputeHash(payload);
			await Task.Delay(800);
			byte[] header = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x00, 0x00, 0x00,0x00,
										 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

			try
			{
				using (var stream = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Write))
				{
					stream.Write(header);
					stream.Write(hash);
					stream.Write(payload);
					stream.Flush();
				}
				return true;
			}
			catch (IOException ex)
			{
				Debug.WriteLine($"Error writing {ex.Message}");

				return false;
			}
		}

		/// <summary>
		/// Build a random buffer
		/// </summary>
		byte[] buildBlock()
		{
			var rand = new Random();
			var buffer = new byte[BlockSize];
			rand.NextBytes(buffer);
			return buffer;
		}
	}
}
