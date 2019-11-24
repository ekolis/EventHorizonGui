using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace EventHorizon
{
	/// <summary>
	/// Wave stream which loops.
	/// http://mark-dot-net.blogspot.com/2009/10/looped-playback-in-net-with-naudio.html
	/// </summary>
	class LoopingWaveStream : WaveStream
	{
		public LoopingWaveStream(WaveStream sourceStream)
		{
			this.sourceStream = sourceStream;
		}

		private WaveStream sourceStream;

		public override int Read(byte[] buffer, int offset, int count)
		{
			int totalBytesRead = 0;

			while (totalBytesRead < count)
			{
				int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
				if (bytesRead == 0)
				{
					if (sourceStream.Position == 0)
					{
						// something wrong with the source stream
						break;
					}
					// loop
					sourceStream.Position = 0;
				}
				totalBytesRead += bytesRead;
			}
			return totalBytesRead;
		}

		public override WaveFormat WaveFormat
		{
			get { return sourceStream.WaveFormat; }
		}

		public override long Length
		{
			get { return sourceStream.Length; }
		}

		public override long Position
		{
			get
			{
				return sourceStream.Position;
			}
			set
			{
				sourceStream.Position = value;
			}
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
				sourceStream.Dispose();
		}
	}
}
