using System.Diagnostics;
using System.Security.Cryptography;

namespace ReadWrite
{
	internal class ReadStuff
	{
		string FileName { get; set; }
		int BlockSize { get; set; }
		CancellationTokenSource cancellationToken;
		readonly object fileLock;

		internal ReadStuff(object lockObj, string fileName, int blockSize, CancellationTokenSource cancelToken)
		{
			FileName = fileName;
			BlockSize = blockSize;
			cancellationToken = cancelToken;
			fileLock = lockObj;
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
				await Task.Delay(1000);
			}
		}

		internal bool DoRead(int blockId)
		{
			const int headerSize = 16;
			int hashSize = 16;
			int blockIdSize = sizeof(int);
			Debug.WriteLine("  ..Try read block " + blockId);
			int recordSize = BlockSize + headerSize + blockIdSize + hashSize;
			var header = new byte[2 * headerSize];
			var blockBuffer = new byte[2*blockIdSize];
			var hash = new byte[2 * hashSize];
			var payload = new byte[2 * BlockSize];
			bool brtn = false;


			if (File.Exists(FileName))
			{
				var fi = new FileInfo(FileName);
				int offset = blockId * recordSize;

				if (Monitor.TryEnter(fileLock))
				{
					try
					{
						using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
						{
							fs.Position = offset;
							var headerCnt = fs.Read(header, 0, headerSize);
							offset += headerSize;
							var blockCnt = fs.Read(blockBuffer, 0, blockIdSize);
							int readBlock = BitConverter.ToInt32(blockBuffer);
							Debug.WriteLine("Read block " + readBlock);
							var hashCnt = fs.Read(hash, 0, hashSize);
							offset += hashSize;
							var payloadCnt = fs.Read(payload, 0, BlockSize);
							MD5 md5 = MD5.Create();
							var payloadHash = md5.ComputeHash(payload);
							brtn = true;
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
//					Monitor.PulseAll(fileLock);
				}
				else
					Debug.WriteLine("   ..doReadAsync timeout");
			}
			return brtn;
		}


	}
}
