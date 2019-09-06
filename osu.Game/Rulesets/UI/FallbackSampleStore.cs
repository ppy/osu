// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.UI
{
    public class FallbackSampleStore : ISampleStore
    {
        private readonly ISampleStore primary;
        private readonly ISampleStore secondary;

        public FallbackSampleStore(ISampleStore primary, ISampleStore secondary)
        {
            this.primary = primary;
            this.secondary = secondary;
        }

        public SampleChannel Get(string name) => primary.Get(name) ?? secondary.Get(name);

        public Task<SampleChannel> GetAsync(string name) => primary.GetAsync(name) ?? secondary.GetAsync(name);

        public Stream GetStream(string name) => primary.GetStream(name) ?? secondary.GetStream(name);

        public IEnumerable<string> GetAvailableResources() => primary.GetAvailableResources().Concat(secondary.GetAvailableResources());

        public void AddAdjustment(AdjustableProperty type, BindableDouble adjustBindable)
        {
            primary.AddAdjustment(type, adjustBindable);
            secondary.AddAdjustment(type, adjustBindable);
        }

        public void RemoveAdjustment(AdjustableProperty type, BindableDouble adjustBindable)
        {
            primary.RemoveAdjustment(type, adjustBindable);
            secondary.RemoveAdjustment(type, adjustBindable);
        }

        public BindableDouble Volume => primary.Volume;

        public BindableDouble Balance => primary.Balance;

        public BindableDouble Frequency => primary.Frequency;

        public int PlaybackConcurrency
        {
            get => primary.PlaybackConcurrency;
            set
            {
                primary.PlaybackConcurrency = value;
                secondary.PlaybackConcurrency = value;
            }
        }

        public void Dispose()
        {
        }
    }
}
