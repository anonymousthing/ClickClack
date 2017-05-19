using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClickClack
{
	public partial class Form1 : Form
	{
		Dictionary<VirtualKeys, bool> lastStates = new Dictionary<VirtualKeys, bool>();
		Dictionary<VirtualKeys, int> soundMappingDown = new Dictionary<VirtualKeys, int>();
		Dictionary<VirtualKeys, int> soundMappingUp = new Dictionary<VirtualKeys, int>();

		Sound[] downSounds;
		Sound[] upSounds;

		Thread audioCleanupThread;

		public Form1()
		{
			InitializeComponent();

			LoadSounds();

			foreach (var key in Enum.GetValues(typeof(VirtualKeys)))
			{
				if (!lastStates.ContainsKey((VirtualKeys)key))
					lastStates.Add((VirtualKeys)key, false);
			}
			
			RawInput.RegisterDevice(HIDUsagePage.Generic, HIDUsage.Keyboard, RawInputDeviceFlags.InputSink, this.Handle);

			audioCleanupThread = new Thread(() =>
			{
				while (true)
				{
					SoundManager.Cleanup();
					Thread.Sleep(50);
				}
			});
			audioCleanupThread.Start();
		}

		public void LoadSounds()
		{
			SoundLoader loader = new SoundLoader();

			List<Sound> downSoundsTemp = new List<Sound>();
			List<Sound> upSoundsTemp = new List<Sound>();

			foreach (string file in Directory.EnumerateFiles("sounds"))
			{
				if (file.EndsWith(".ogg"))
				{
					if (file.Contains("down"))
						downSoundsTemp.Add(loader.LoadSound(file));
					else if (file.Contains("up"))
						upSoundsTemp.Add(loader.LoadSound(file));
				}
			}

			downSounds = downSoundsTemp.ToArray();
			upSounds = upSoundsTemp.ToArray();

			Random random = new Random();
			foreach (var key in Enum.GetValues(typeof(VirtualKeys)))
			{
				if (!soundMappingDown.ContainsKey((VirtualKeys)key))
				{
					soundMappingDown.Add((VirtualKeys)key, random.Next(downSounds.Length));
					soundMappingUp.Add((VirtualKeys)key, random.Next(upSounds.Length));
				}
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			audioCleanupThread.Abort();
		}

		protected override void WndProc(ref Message m)
		{
			WM message = (WM)m.Msg;
			switch (message)
			{
				case WM.GETICON:
					OnInput(new KeyPressEventArgs() { Key = VirtualKeys.Tab, Pressed = true });
					break;
				case WM.INPUT:
					RawInput.ProcessMessage(m.LParam, OnInput);
					break;
				default:
					int i = 5;
					break;
			}
			base.WndProc(ref m);
		}

		private void OnInput(KeyPressEventArgs e)
		{
			Console.Write(e.Key.ToString() + " = " + e.Pressed);
			var lastState = lastStates[e.Key];
			if (e.Pressed != lastState)
			{
				int soundIndex;
				if (e.Pressed)
				{
					soundIndex = soundMappingDown[e.Key];
					SoundManager.Play(downSounds[soundIndex]);
					lastStates[e.Key] = true;
				}
				else
				{
					soundIndex = soundMappingUp[e.Key];
					SoundManager.Play(upSounds[soundIndex]);
					lastStates[e.Key] = false;
				}

				Console.WriteLine("; " + soundIndex);
			}
		}
	}
}
