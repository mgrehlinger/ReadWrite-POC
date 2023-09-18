#define SIMULTANEOUS

namespace ReadWrite
{
#pragma warning disable CS4014 // don't nag about not awaiting
	internal class Program
	{
		public static class Locker
		{
			public static ReaderWriterLock Rw { get; set; }
			public static int TimeOut { get; set; }
		}


		static async Task Main(string[] args)
		{
			CancellationTokenSource zoneCancel = new(); // Used to exit data handling thread
			int blockSize = 500_000;
			string fileName = @"c:\temp\readwrite.bin";
			Locker.Rw = new ReaderWriterLock();
			Locker.TimeOut = 200;



			if ( File.Exists(fileName))
				File.Delete(fileName);

			Console.WriteLine("Hello, World!");
			var rs = new ReadStuff(fileName, blockSize, zoneCancel);
			var ws = new WriteStuff(fileName, blockSize, zoneCancel);

#if SIMULTANEOUS
			Task.Run(() => { ws.WriteLoopAsync(); });
			Task.Run(() => { rs.ReadLoopAsync(); });

			Console.WriteLine("reading");
			await Task.Delay(40_000);
			zoneCancel.Cancel();
			await Task.Delay(1000);

#else
			for ( int i = 0; i < 10;  i++ )
			{
				ws.DoWrite(i);
				rs.DoRead(i);
			}

#endif

			Console.WriteLine("***done***");

		}
	}
#pragma warning restore CS4014
}