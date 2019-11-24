using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;

namespace EventHorizon
{
	/// <summary>
	/// Wave stream that doesn't contain any audio.
	/// </summary>
	class EmptyWaveStream : WaveStream
	{
		public override WaveFormat WaveFormat
		{
			get { return WaveFormat.CreateIeeeFloatWaveFormat(44100, 2); }
		}

		public override long Length
		{
			get { return 0; }
		}

		public override long Position
		{
			get
			{
				return 0;
			}
			set
			{
				// do nothing, who cares?
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return 0;
		}
	}
}
