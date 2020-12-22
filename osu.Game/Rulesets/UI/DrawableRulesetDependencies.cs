// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Rulesets.Configuration;

namespace osu.Game.Rulesets.UI
{
    public class DrawableRulesetDependencies : DependencyContainer, IDisposable
    {
        /// <summary>
        /// The texture store to be used for the ruleset.
        /// </summary>
        public TextureStore TextureStore { get; }

        /// <summary>
        /// The sample store to be used for the ruleset.
        /// </summary>
        /// <remarks>
        /// This is the local sample store pointing to the ruleset sample resources,
        /// the cached sample store (<see cref="FallbackSampleStore"/>) retrieves from
        /// this store and falls back to the parent store if this store doesn't have the requested sample.
        /// </remarks>
        public ISampleStore SampleStore { get; }

        /// <summary>
        /// The ruleset config manager.
        /// </summary>
        public IRulesetConfigManager RulesetConfigManager { get; private set; }

        public DrawableRulesetDependencies(Ruleset ruleset, IReadOnlyDependencyContainer parent)
            : base(parent)
        {
            var resources = ruleset.CreateResourceStore();

            if (resources != null)
            {
                TextureStore = new TextureStore(parent.Get<GameHost>().CreateTextureLoaderStore(new NamespacedResourceStore<byte[]>(resources, @"Textures")));
                CacheAs(TextureStore = new FallbackTextureStore(TextureStore, parent.Get<TextureStore>()));

                SampleStore = parent.Get<AudioManager>().GetSampleStore(new NamespacedResourceStore<byte[]>(resources, @"Samples"));
                SampleStore.PlaybackConcurrency = OsuGameBase.SAMPLE_CONCURRENCY;
                CacheAs(SampleStore = new FallbackSampleStore(SampleStore, parent.Get<ISampleStore>()));
            }

            RulesetConfigManager = parent.Get<RulesetConfigCache>().GetConfigFor(ruleset);
            if (RulesetConfigManager != null)
                Cache(RulesetConfigManager);
        }

        #region Disposal

        ~DrawableRulesetDependencies()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;

            SampleStore?.Dispose();
            TextureStore?.Dispose();
            RulesetConfigManager = null;
        }

        #endregion

        /// <summary>
        /// A sample store which adds a fallback source and prevents disposal of the fallback source.
        /// </summary>
        private class FallbackSampleStore : ISampleStore
        {
            private readonly ISampleStore primary;
            private readonly ISampleStore fallback;

            public FallbackSampleStore(ISampleStore primary, ISampleStore fallback)
            {
                this.primary = primary;
                this.fallback = fallback;
            }

            public SampleChannel Get(string name) => primary.Get(name) ?? fallback.Get(name);

            public Task<SampleChannel> GetAsync(string name) => primary.GetAsync(name) ?? fallback.GetAsync(name);

            public Stream GetStream(string name) => primary.GetStream(name) ?? fallback.GetStream(name);

            public IEnumerable<string> GetAvailableResources() => throw new NotSupportedException();

            public void AddAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable) => throw new NotSupportedException();

            public void RemoveAdjustment(AdjustableProperty type, BindableNumber<double> adjustBindable) => throw new NotSupportedException();

            public void RemoveAllAdjustments(AdjustableProperty type) => throw new NotSupportedException();

            public BindableNumber<double> Volume => throw new NotSupportedException();

            public BindableNumber<double> Balance => throw new NotSupportedException();

            public BindableNumber<double> Frequency => throw new NotSupportedException();

            public BindableNumber<double> Tempo => throw new NotSupportedException();

            public IBindable<double> GetAggregate(AdjustableProperty type) => throw new NotSupportedException();

            public IBindable<double> AggregateVolume => throw new NotSupportedException();

            public IBindable<double> AggregateBalance => throw new NotSupportedException();

            public IBindable<double> AggregateFrequency => throw new NotSupportedException();

            public IBindable<double> AggregateTempo => throw new NotSupportedException();

            public int PlaybackConcurrency
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public void Dispose()
            {
                primary?.Dispose();
            }
        }

        /// <summary>
        /// A texture store which adds a fallback source and prevents disposal of the fallback source.
        /// </summary>
        private class FallbackTextureStore : TextureStore
        {
            private readonly TextureStore primary;
            private readonly TextureStore fallback;

            public FallbackTextureStore(TextureStore primary, TextureStore fallback)
            {
                this.primary = primary;
                this.fallback = fallback;
            }

            public override Texture Get(string name, WrapMode wrapModeS, WrapMode wrapModeT)
                => primary.Get(name, wrapModeS, wrapModeT) ?? fallback.Get(name, wrapModeS, wrapModeT);

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                primary?.Dispose();
            }
        }
    }
}
