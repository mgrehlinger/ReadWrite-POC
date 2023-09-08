namespace ReadWrite
{
#pragma warning disable CS4014 // don't nag about not awaiting
	internal class Program
	{


		static async Task Main(string[] args)
		{
			CancellationTokenSource zoneCancel = new(); // Used to exit data handling thread
			int blockSize = 992;
			string fileName = @"c:\temp\readwrite.bin";
			
//			if ( File.Exists(fileName))
//				File.Delete(fileName);

			Console.WriteLine("Hello, World!");
			var rs = new ReadStuff(fileName, blockSize, zoneCancel);
			var ws = new WriteStuff(fileName, blockSize, zoneCancel);
			Task.Run(() => { rs.ReadLoopAsync(); });
//			Task.Run(() => { ws.WriteLoopAsync(); });

			Console.WriteLine("reading");
			await Task.Delay(10000);
			zoneCancel.Cancel();
			await Task.Delay(1000);
			Console.WriteLine("***done***");

		}
	}
#pragma warning restore CS4014
}