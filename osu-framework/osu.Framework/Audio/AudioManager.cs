//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Collections.Generic;
using System.IO;
using ManagedBass;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Resources;
using osu.Framework.Threading;

namespace osu.Framework.Audio
{
    public class AudioManager : AudioCollectionManager<AdjustableAudioComponent>
    {
        public TrackManager Track => GetTrackManager();
        public SampleManager Sample => GetSampleManager();

        internal event VoidDelegate AvailableDevicesChanged;

        internal List<DeviceInfo> AudioDevices = new List<DeviceInfo>();

        internal string CurrentAudioDevice;

        private string lastPreferredDevice;

        /// <summary>
        /// Volume of all samples played game-wide.
        /// </summary>
        public readonly BindableDouble VolumeSample = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        /// <summary>
        /// Volume of all tracks played game-wide.
        /// </summary>
        public readonly BindableDouble VolumeTrack = new BindableDouble(1) { MinValue = 0, MaxValue = 1 };

        private Scheduler scheduler = new Scheduler();

        public AudioManager(IResourceStore<byte[]> trackStore, IResourceStore<byte[]> sampleStore)
        {
            globalTrackManager = GetTrackManager(trackStore);
            globalSampleManager = GetSampleManager(sampleStore);

            SetAudioDevice();

            scheduler.AddDelayed(checkAudioDeviceChanged, 1000, true);
        }

        private TrackManager globalTrackManager;
        private SampleManager globalSampleManager;

        public TrackManager GetTrackManager(IResourceStore<byte[]> store = null)
        {
            if (store == null) return globalTrackManager;

            TrackManager tm = new TrackManager(store);
            AddItem(tm);
            tm.AddAdjustment(AdjustableProperty.Volume, VolumeTrack);

            return tm;
        }

        public SampleManager GetSampleManager(IResourceStore<byte[]> store = null)
        {
            if (store == null) return globalSampleManager;

            SampleManager sm = new SampleManager(store);
            AddItem(sm);
            sm.AddAdjustment(AdjustableProperty.Volume, VolumeSample);

            return sm;
        }

        internal bool CheckAudioDevice()
        {
            if (CurrentAudioDevice != null)
                return true;

            //NotificationManager.ShowMessage("No compatible audio device detected. You must plug in a valid audio device in order to play osu!", Color4.Red, 4000);
            return false;
        }

        private List<DeviceInfo> getAllDevices()
        {
            int deviceCount = Bass.DeviceCount;
            List<DeviceInfo> info = new List<DeviceInfo>();
            for (int i = 0; i < deviceCount; i++)
                info.Add(Bass.GetDeviceInfo(i));

            return info;
        }

        public bool SetAudioDevice(string preferredDevice = null)
        {
            lastPreferredDevice = preferredDevice;

            AudioDevices = new List<DeviceInfo>(getAllDevices());
            AvailableDevicesChanged?.Invoke();

            string oldDevice = CurrentAudioDevice;
            string newDevice = preferredDevice;

            if (string.IsNullOrEmpty(newDevice))
                newDevice = AudioDevices.Find(df => df.IsDefault).Name;

            bool oldDeviceValid = Bass.CurrentDevice >= 0;
            if (oldDeviceValid)
            {
                DeviceInfo oldDeviceInfo = Bass.GetDeviceInfo(Bass.CurrentDevice);
                oldDeviceValid &= oldDeviceInfo.IsEnabled && oldDeviceInfo.IsInitialized;
            }

            if (newDevice == oldDevice)
            {
                //check the old device is still valid
                if (oldDeviceValid)
                    return true;
            }

            if (string.IsNullOrEmpty(newDevice))
                return false;

            int newDeviceIndex = AudioDevices.FindIndex(df => df.Name == newDevice);


            DeviceInfo newDeviceInfo = new DeviceInfo();

            try
            {
                if (newDeviceIndex >= 0)
                    newDeviceInfo = Bass.GetDeviceInfo(newDeviceIndex);
                //we may have previously initialised this device.
            }
            catch
            {
            }

            if (oldDeviceValid && (newDeviceInfo.Driver == null || !newDeviceInfo.IsEnabled))
            {
                //handles the case we are trying to load a user setting which is currently unavailable,
                //and we have already fallen back to a sane default.
                return true;
            }

            if (newDevice != null && oldDevice != null)
            {
                //we are preparing to load a new device, so let's clean up any existing device.
                clearAllCaches();
                Bass.Free();
            }

            if (!Bass.Init(newDeviceIndex, 44100, 0, Game.Window.Handle))
            {
                //the new device didn't go as planned. we need another option.

                if (preferredDevice == null)
                {
                    //we're fucked. the default device won't initialise.
                    CurrentAudioDevice = null;
                    return false;
                }

                //let's try again using the default device.
                return SetAudioDevice();
            }

            //we have successfully initialised a new device.
            CurrentAudioDevice = newDevice;

            Bass.PlaybackBufferLength = 100;
            Bass.UpdatePeriod = 5;

            return true;
        }

        private void clearAllCaches()
        {

        }

        private int lastDeviceCount;

        private void checkAudioDeviceChanged()
        {
            bool useDefault = string.IsNullOrEmpty(lastPreferredDevice);

            if (useDefault)
            {
                int currentDevice = Bass.CurrentDevice;
                try
                {
                    DeviceInfo device = Bass.GetDeviceInfo(currentDevice);
                    if (device.IsDefault && device.IsEnabled)
                        return; //early return when nothing has changed.
                }
                catch
                {
                    return;
                }
            }

            int availableDevices = 0;

            foreach (DeviceInfo device in getAllDevices())
            {
                if (device.Driver == null) continue;

                bool isCurrentDevice = device.Name == CurrentAudioDevice;

                if (device.IsEnabled)
                {
                    if (isCurrentDevice && !device.IsDefault && useDefault)
                        //the default device on windows has changed, so we need to update.
                        SetAudioDevice();
                    availableDevices++;
                }
                else if (isCurrentDevice)
                    SetAudioDevice(lastPreferredDevice);
                //the active device has been disabled.
            }

            if (lastDeviceCount != availableDevices && lastDeviceCount > 0)
            {
                SetAudioDevice(lastPreferredDevice);

                //just update the available devices.
                //if (availableDevices > lastDeviceCount)
                    //NotificationManager.ShowMessage(LocalisationManager.GetString(OsuString.AudioEngine_NewDeviceDetected), Color4.YellowGreen, 5000);
            }

            lastDeviceCount = availableDevices;
        }

        public override void Update()
        {
            base.Update();

            scheduler.Update();
        }
    }
}
