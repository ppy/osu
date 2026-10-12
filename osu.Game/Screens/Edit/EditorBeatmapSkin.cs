// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit
{
    /// <summary>
    /// A beatmap skin which is being edited.
    /// </summary>
    public class EditorBeatmapSkin : ISkin, IDisposable
    {
        /// <summary>
        /// Invoked when the beatmap skin changes.
        /// This event is not locally scheduled to update thread or otherwise marshalled
        /// in a way that would prevent invocation of a callback registered by a potentially-now-disposed caller.
        /// Callers are expected to schedule locally as required.
        /// </summary>
        public event Action? BeatmapSkinChanged;

        /// <summary>
        /// The underlying beatmap skin.
        /// </summary>
        protected internal readonly LegacyBeatmapSkin Skin;

        /// <summary>
        /// The combo colours of this skin.
        /// If empty, the default combo colours will be used.
        /// </summary>
        public BindableList<Colour4> ComboColours { get; }

        private readonly EditorBeatmap editorBeatmap;

        public EditorBeatmapSkin(EditorBeatmap editorBeatmap, LegacyBeatmapSkin skin)
        {
            this.editorBeatmap = editorBeatmap;

            Skin = skin;
            ComboColours = new BindableList<Colour4>();

            if (Skin.Configuration.ComboColours is IReadOnlyList<Color4> comboColours)
            {
                // due to the foibles of how `IHasComboInformation` / `ComboIndexWithOffsets` work,
                // the actual effective first combo colour that will be used on the beatmap is the one with index 1, not 0.
                // see also: `IHasComboInformation.UpdateComboInformation`,
                // https://github.com/peppy/osu-stable-reference/blob/46cd3a10af7cc6cc96f4eba92ef1812dc8c3a27e/osu!/GameModes/Edit/Forms/SongSetup.cs#L233-L234.
                for (int i = 0; i < comboColours.Count; ++i)
                    ComboColours.Add(comboColours[(i + 1) % comboColours.Count]);
            }

            ComboColours.BindCollectionChanged((_, _) => updateColours());

            if (skin.BeatmapSetResources != null)
                skin.BeatmapSetResources.CacheInvalidated += beatmapResourcesInvalidated;
        }

        private void beatmapResourcesInvalidated()
        {
            Skin.RecycleSamples();
            InvokeSkinChanged();
        }

        public void InvokeSkinChanged() => BeatmapSkinChanged?.Invoke();

        #region Combo colours

        private void updateColours()
        {
            // performs the inverse of the index rotation operation described in the ctor.
            Skin.Configuration.CustomComboColours.Clear();
            for (int i = 0; i < ComboColours.Count; ++i)
                Skin.Configuration.CustomComboColours.Add(ComboColours[(ComboColours.Count + i - 1) % ComboColours.Count]);
            InvokeSkinChanged();
            editorBeatmap.SaveState();
        }

        #endregion

        #region Sample sets

        public record SampleSet(int SampleSetIndex, string Name)
        {
            public SampleSet(int sampleSetIndex)
                : this(sampleSetIndex, $@"Custom #{sampleSetIndex}")
            {
            }

            public override string ToString() => Name;

            public HashSet<string> Filenames = [];

            public string? FindSampleIfExists(string sampleName, string bankName)
                => Filenames.SingleOrDefault(f => f.StartsWith($@"{bankName}-{sampleName}{(SampleSetIndex > 1 ? SampleSetIndex : null)}", StringComparison.Ordinal));

            public virtual bool Equals(SampleSet? other) => SampleSetIndex == other?.SampleSetIndex;
            public override int GetHashCode() => SampleSetIndex;
        }

        public IEnumerable<SampleSet> GetAvailableSampleSets()
        {
            string[] possibleSounds = HitSampleInfo.ALL_ADDITIONS.Prepend(HitSampleInfo.HIT_NORMAL).ToArray();
            string[] possibleBanks = HitSampleInfo.ALL_BANKS;

            string[] possiblePrefixes = possibleSounds.SelectMany(sound => possibleBanks.Select(bank => $@"{bank}-{sound}")).ToArray();

            Dictionary<int, SampleSet> sampleSets = new Dictionary<int, SampleSet>
            {
                [1] = new SampleSet(1),
            };

            if (Skin.Samples != null)
            {
                foreach (string sample in Skin.Samples.GetAvailableResources())
                {
                    foreach (string possiblePrefix in possiblePrefixes)
                    {
                        if (!sample.StartsWith(possiblePrefix, StringComparison.Ordinal))
                            continue;

                        string indexString = Path.GetFileNameWithoutExtension(sample)[possiblePrefix.Length..];
                        int? index = null;

                        if (string.IsNullOrEmpty(indexString))
                            index = 1;
                        if (int.TryParse(indexString, out int parsed) && parsed >= 2)
                            index = parsed;

                        if (!index.HasValue)
                            continue;

                        SampleSet? sampleSet;
                        if (!sampleSets.TryGetValue(index.Value, out sampleSet))
                            sampleSet = sampleSets[index.Value] = new SampleSet(index.Value);

                        sampleSet.Filenames.Add(sample);
                    }
                }
            }

            return sampleSets.OrderBy(i => i.Key).Select(i => i.Value);
        }

        #endregion

        public void Dispose()
        {
            if (Skin.BeatmapSetResources != null)
                Skin.BeatmapSetResources.CacheInvalidated -= beatmapResourcesInvalidated;
            Skin.Dispose();
        }

        #region Delegated ISkin implementation

        public Drawable? GetDrawableComponent(ISkinComponentLookup lookup) => Skin.GetDrawableComponent(lookup);
        public Texture? GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => Skin.GetTexture(componentName, wrapModeS, wrapModeT);
        public ISample? GetSample(ISampleInfo sampleInfo) => Skin.GetSample(sampleInfo);

        public IBindable<TValue>? GetConfig<TLookup, TValue>(TLookup lookup)
            where TLookup : notnull
            where TValue : notnull
            => Skin.GetConfig<TLookup, TValue>(lookup);

        #endregion
    }
}
