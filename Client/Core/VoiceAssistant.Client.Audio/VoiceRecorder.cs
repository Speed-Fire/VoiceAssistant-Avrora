using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoiceAssistant.Client.Abstractions;

namespace VoiceAssistant.Client.Audio
{
	internal class VoiceRecorder : IVoiceRecorder
	{
		private const sbyte SILENCE_TRESHOLD = -40;
		private const float MAX_SILENCE_DURATION = 1.5f; // in seconds

		private readonly WaveInEvent _waveIn = new();

		private WaveFileWriter? _waveFileWriter;

		private volatile int _silentSampleCount = 0;
		private volatile bool _recorded = false;
		private bool _disposing = false;

		public VoiceRecorder()
		{
			_waveIn.WaveFormat = new WaveFormat(44100, 1);

			_waveIn.DataAvailable += OnDataAvailable;
			_waveIn.RecordingStopped += OnRecordingStopped;
		}

		public Task<Stream> RecordAsync(CancellationToken stoppingToken)
		{
			return Task.Run<Stream>(() => Record(stoppingToken), stoppingToken);
		}

		private MemoryStream Record(CancellationToken stoppingToken)
		{
			var cancelled = false;

			_recorded = false;
			var outputStream = new MemoryStream();
			_waveFileWriter = new(outputStream, _waveIn.WaveFormat);
			_waveIn.StartRecording();

			while (true)
			{
				if(stoppingToken.IsCancellationRequested && !cancelled)
				{
					cancelled = true;
					_waveIn.StopRecording();
				}

				if (_recorded)
					break;
			}

			return outputStream;
		}

		private void OnDataAvailable(object? sender, WaveInEventArgs e)
		{
			_waveFileWriter!.Write(e.Buffer, 0, e.BytesRecorded);

			AnalyzeSilense(e.Buffer.AsSpan(0, e.BytesRecorded));

			if(_waveFileWriter.Position > _waveIn.WaveFormat.AverageBytesPerSecond * 10 ||
				_silentSampleCount / _waveIn.WaveFormat.SampleRate > MAX_SILENCE_DURATION)
			{
				_waveIn.StopRecording();
			}
		}

		private void OnRecordingStopped(object? sender, StoppedEventArgs e)
		{
			_waveFileWriter?.Dispose();
			_waveFileWriter = null;

			if (_disposing)
			{
				_waveIn.Dispose();
			}

			_recorded = true;
		}

		private unsafe void AnalyzeSilense(ReadOnlySpan<byte> data)
		{
			fixed(void* ptr = data)
			{
				Int16* buffer = (Int16*)ptr;

				var isSilent = true;

				for(int i = 0; i < data.Length / 2; i++)
				{
					if (!IsAmplitudeSilent(buffer[i] / 32768f, SILENCE_TRESHOLD))
					{
						isSilent = false;
						break;
					}
				}

				_silentSampleCount = isSilent ? _silentSampleCount + 1 : 0;
			}
		}

		private static bool IsAmplitudeSilent(float amplitude, sbyte threshold)
		{
			double dB = 20 * Math.Log10(Math.Abs(amplitude));
			return dB < threshold;
		}

		public void Dispose()
		{
			_disposing = true;
			_waveIn.StopRecording();
		}
	}
}
