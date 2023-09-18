using System.Diagnostics;
using System.Security.Cryptography;
using static ReadWrite.Program;

namespace ReadWrite
{
	internal class ReadStuff
	{
		string FileName { get; set; }
		int BlockSize { get; set; }
		CancellationTokenSource cancellationToken;

		internal ReadStuff(string fileName, int blockSize, CancellationTokenSource cancelToken)
		{
			FileName = fileName;
			BlockSize = blockSize;
			cancellationToken = cancelToken;
		}

		internal async Task ReadLoopAsync()
		{
			int blockCnt = 0;
			while (true)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					Console.WriteLine("Cancel ReadStuff");
					break;
				}
				bool bRead = DoRead(blockCnt);
				if (bRead) blockCnt++;
				await Task.Delay(120);
			}
		}

		internal bool DoRead(int blockId)
		{
			const int headerSize = 16;
			int hashSize = 16;
			int blockIdSize = sizeof(int);
			int recordSize = BlockSize + headerSize + blockIdSize + hashSize;
			var header = new byte[headerSize];
			var blockBuffer = new byte[blockIdSize];
			var hash = new byte[hashSize];
			var payload = new byte[BlockSize];
			bool brtn = false;


			if (File.Exists(FileName))
			{
				try
				{
					Locker.Rw.AcquireReaderLock((int)Locker.TimeOut);
					try
					{
						using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							int offset = blockId * recordSize;
							fs.Position = offset;
							var headerCnt = fs.Read(header, 0, headerSize);
							offset += headerSize;
							var blockCnt = fs.Read(blockBuffer, 0, blockIdSize);
							int readBlock = BitConverter.ToInt32(blockBuffer);
							var hashCnt = fs.Read(hash, 0, hashSize);
							offset += hashSize;
							var payloadCnt = fs.Read(payload, 0, BlockSize);
							MD5 md5 = MD5.Create();
							var payloadHash = md5.ComputeHash(payload);
							brtn = payloadHash.SequenceEqual(hash);
							Debug.WriteLine("DoRead: " + readBlock + " brtn = " + brtn);
						}
					}
					catch (IOException ex)
					{
						Debug.WriteLine($"IOException reading {ex.Message}");
					}
					catch (Exception ex)
					{
						Debug.WriteLine($"Other Error reading {ex.Message}");
					}

				}
				finally { Locker.Rw.ReleaseReaderLock(); }


			}
			return brtn;
		}


	}
}
