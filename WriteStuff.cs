using System.Diagnostics;
using System.Security.Cryptography;
using static ReadWrite.Program;

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
				bool bSuccess = DoWrite(blockCnt);
				if (bSuccess) blockCnt++;
				await Task.Delay(100);
			}
		}

		internal bool DoWrite(int blockId)
		{
			var payload = buildBlock();
			var md5 = MD5.Create();
			var hash = md5.ComputeHash(payload);
			byte[] header = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF, 0x00, 0x00, 0x00,0x00,
										 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
			bool brtn = false;
			try
			{
				Locker.Rw.AcquireWriterLock((int)Locker.TimeOut);
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
			}
			finally { Locker.Rw.ReleaseWriterLock(); }
			if (!brtn)
			{
				Debug.WriteLine("DoWrite  FAILED: " + blockId + " brtn = " + brtn);
			}
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
