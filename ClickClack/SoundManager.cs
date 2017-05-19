using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickClack
{
	class SoundManager
	{
		static AudioContext context;

		static List<int> sources = new List<int>();

		static SoundManager()
		{
			context = new AudioContext();
		}

		public static void Play(Sound sound)
		{
			int source = AL.GenSource();

			int buffer = AL.GenBuffer();
			AL.BufferData(buffer, ALFormat.Stereo16, sound.Data, sound.Data.Length, SoundLoader.SAMPLE_RATE);

			AL.SourceQueueBuffer(source, buffer);
			AL.SourcePlay(source);
			lock (sources)
			{
				sources.Add(source);
			}
		}

		public static void PlaySequential(params Sound[] sounds)
		{
			int source = AL.GenSource();

			int[] buffers = AL.GenBuffers(sounds.Length);
			for (int i = 0; i < sounds.Length; i++)
				AL.BufferData(buffers[i], ALFormat.Stereo16, sounds[i].Data, sounds[i].Data.Length, SoundLoader.SAMPLE_RATE);

			AL.SourceQueueBuffers(source, sounds.Length, buffers);
			AL.SourcePlay(source);
			lock (sources)
			{
				sources.Add(source);
			}
		}

		public static void Cleanup()
		{
			lock (sources)
			{
				foreach (var source in sources)
				{
					//Clean out processed buffers
					int processed;
					AL.GetSource(source, ALGetSourcei.BuffersProcessed, out processed);
					if (processed > 0)
					{
						int[] buffersToDispose = AL.SourceUnqueueBuffers(source, processed);
						AL.DeleteBuffers(buffersToDispose);
					}

					//Cleanup source if over
					int state;
					AL.GetSource(source, ALGetSourcei.SourceState, out state);
					if ((ALSourceState)state == ALSourceState.Stopped)
					{
						AL.DeleteSource(source);
						//TODO: call sound over event
					}
				}
			}
		}
	}
}
