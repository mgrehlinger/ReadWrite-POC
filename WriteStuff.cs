using System.Diagnostics;
using System.Security.Cryptography;

namespace ReadWrite
{
	internal class WriteStuff
	{
		string FileName { get; set; }
		int BlockSize { get; set; }
		CancellationTokenSource cancellationToken;
		readonly object fileLock;

		internal WriteStuff(object lockObj, string fileName, int blockSize, CancellationTokenSource cancelToken)
		{
			FileName = fileName;
			BlockSize = blockSize;
			cancellationToken = cancelToken;
			fileLock = lockObj;
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
				bool bSuccess = DoWrite(blockCnt);
				if (bSuccess) blockCnt++;
				await Task.Delay(400);
			}
		}

		internal bool DoWrite(int blockId)
		{
			Debug.WriteLine("  ..Write block " + blockId);
			var payload = buildBlock();
			var md5 = MD5.Create();
			var hash = md5.ComputeHash(payload);
			byte[] header = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x00, 0x00, 0x00,0x00,
										 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
			bool brtn = false;
			if (Monitor.TryEnter(fileLock))
			{
				try
				{
					using (var stream = new FileStream(FileName, FileMode.Append, FileAccess.Write, FileShare.Write))
					{
						stream.Write(header);
						stream.Write(BitConverter.GetBytes(blockId));
						stream.Write(hash);
						stream.Write(payload);
						stream.Flush();
					}
					brtn = true;
				}
				catch (IOException ex)
				{
					Debug.WriteLine($"Error writing {ex.Message}");
				}
//				Monitor.PulseAll(fileLock);
			}
			else
				Debug.WriteLine("WRITE timeout block " + blockId);
			return brtn;
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
