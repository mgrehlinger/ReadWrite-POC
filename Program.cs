namespace ReadWrite
{
#pragma warning disable CS4014 // don't nag about not awaiting
	internal class Program
	{


		static async Task Main(string[] args)
		{
			CancellationTokenSource zoneCancel = new(); // Used to exit data handling thread
			int blockSize = 50_000;
			string fileName = @"c:\temp\readwrite.bin";
			object fileLock = new();


			
			if ( File.Exists(fileName))
				File.Delete(fileName);

			Console.WriteLine("Hello, World!");
			var rs = new ReadStuff(fileLock, fileName, blockSize, zoneCancel);
			var ws = new WriteStuff(fileLock, fileName, blockSize, zoneCancel);
/*
			Task.Run(() => { ws.WriteLoopAsync(); });
			Task.Run(() => { rs.ReadLoopAsync(); });

			Console.WriteLine("reading");
			await Task.Delay(40_000);
			zoneCancel.Cancel();
			await Task.Delay(1000);
*/
			for ( int i = 0; i < 10;  i++ )
			{
				ws.DoWrite(i);
				rs.DoRead(i);
			}
			Console.WriteLine("***done***");

		}
	}
#pragma warning restore CS4014
}