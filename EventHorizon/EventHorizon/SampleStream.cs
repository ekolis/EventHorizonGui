using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace EventHorizon
{
	class SampleStream : WaveStream
	{
		public SampleStream(ISampleProvider sampler, long length)
		{
			this.sampler = new SampleToWaveProvider(sampler);
			this.length = length;
		}

		private SampleToWaveProvider sampler;
		private long length;

		public override WaveFormat WaveFormat
		{
			get { return sampler.WaveFormat; }
		}

		public override long Length
		{
			get { return length; }
		}

		public override long Position
		{
			get
			{
				return 0;
			}
			set
			{
				// irrelevant
			}
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return sampler.Read(buffer, offset, count);
		}
	}
}
