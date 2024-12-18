// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.IO;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Skinning;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests
{
    /// <summary>
    /// A <see cref="WorkingBeatmap"/> that is used for test scenes that include waveforms.
    /// </summary>
    public class WaveformTestBeatmap : WorkingBeatmap
    {
        private readonly Beatmap beatmap;
        private readonly ITrackStore trackStore;

        public WaveformTestBeatmap(AudioManager audioManager, RulesetInfo rulesetInfo = null)
            : this(audioManager, new TestBeatmap(rulesetInfo ?? new OsuRuleset().RulesetInfo))
        {
        }

        public WaveformTestBeatmap(AudioManager audioManager, Beatmap beatmap)
            : base(beatmap.BeatmapInfo, audioManager)
        {
            this.beatmap = beatmap;
            trackStore = audioManager.GetTrackStore(getZipReader());
        }

        ~WaveformTestBeatmap()
        {
            // Remove the track store from the audio manager
            trackStore?.Dispose();
        }

        private static Stream getStream() => TestResources.GetTestBeatmapStream();

        private static ZipArchiveReader getZipReader() => new ZipArchiveReader(getStream());

        protected override IBeatmap GetBeatmap() => beatmap;

        public override Texture GetBackground() => null;

        protected override Waveform GetWaveform() => new Waveform(trackStore.GetStream(firstAudioFile));

        protected internal override ISkin GetSkin() => null;

        public override Stream GetStream(string storagePath) => null;

        protected override Track GetBeatmapTrack() => trackStore.Get(firstAudioFile);

        public override bool TryTransferTrack(WorkingBeatmap target)
        {
            // Our track comes from a local track store that's disposed on finalizer,
            // therefore it's unsafe to transfer it to another working beatmap.
            return false;
        }

        private string firstAudioFile
        {
            get
            {
                using (var reader = getZipReader())
                    return reader.Filenames.First(f => f.EndsWith(".mp3", StringComparison.Ordinal));
            }
        }
    }
}
