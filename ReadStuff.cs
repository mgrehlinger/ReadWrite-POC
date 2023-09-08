using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
				await doReadAsync(blockCnt++);
			}
		}

		async Task<bool> doReadAsync(int blockId)
		{
			const int headerSize = 16;
			int hashSize = 16;
			Debug.WriteLine("  ..read block " + blockId);
			int recordSize = BlockSize + headerSize + hashSize;
			var header = new byte[2*headerSize];
			var hash = new byte[2*hashSize];
			var payload = new byte[2*BlockSize];
			bool brtn = false;


			if (File.Exists(FileName))
			{
				var fi = new FileInfo(FileName);
				int offset = blockId * recordSize;
				Debug.WriteLine("  ..BlockId: " + blockId + " FileSize: " + fi.Length.ToString("N0") + " offset: " + offset);

				try
				{
					using (var fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						fs.Position = offset;
						var headerCnt = fs.Read(header, 0, headerSize);
						offset += headerSize;
						var hashCnt = fs.Read(hash, 0, hashSize);
						offset += hashSize;
						var payloadCnt = fs.Read(payload, 0, BlockSize);
						Debug.WriteLine("  ..Read block: " + blockId);
						MD5 md5 = MD5.Create();
						var payloadHash = md5.ComputeHash(payload);
						brtn = true;
					}
				}
				catch (IOException ex)
				{
					Debug.WriteLine($"IOException reading {ex.Message}");	
				}
				catch(Exception ex)
				{
					Debug.WriteLine($"Other Error reading {ex.Message}");
				}
			}
			await Task.Delay(2000);
			return brtn;
		}


	}
}
