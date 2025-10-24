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
    public class EditorBeatmapSkin : ISkin
    {
        public event Action? BeatmapSkinChanged;

        /// <summary>
        /// The underlying beatmap skin.
        /// </summary>
        protected internal readonly Skin Skin;

        /// <summary>
        /// The combo colours of this skin.
        /// If empty, the default combo colours will be used.
        /// </summary>
        public BindableList<Colour4> ComboColours { get; }

        public EditorBeatmapSkin(Skin skin)
        {
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
        }

        private void invokeSkinChanged() => BeatmapSkinChanged?.Invoke();

        private void updateColours()
        {
            // performs the inverse of the index rotation operation described in the ctor.
            Skin.Configuration.CustomComboColours.Clear();
            for (int i = 0; i < ComboColours.Count; ++i)
                Skin.Configuration.CustomComboColours.Add(ComboColours[(ComboColours.Count + i - 1) % ComboColours.Count]);
            invokeSkinChanged();
        }

        public record SampleSet(int SampleSetIndex, string Name)
        {
            public SampleSet(int sampleSetIndex)
                : this(sampleSetIndex, $@"Custom #{sampleSetIndex}")
            {
            }

            public override string ToString() => Name;
        }

        public IEnumerable<SampleSet> GetAvailableSampleSets()
        {
            string[] possibleSounds = HitSampleInfo.ALL_ADDITIONS.Prepend(HitSampleInfo.HIT_NORMAL).ToArray();
            string[] possibleBanks = HitSampleInfo.ALL_BANKS;

            string[] possiblePrefixes = possibleSounds.SelectMany(sound => possibleBanks.Select(bank => $@"{bank}-{sound}")).ToArray();

            HashSet<int> indices = new HashSet<int>();

            if (Skin.Samples != null)
            {
                foreach (string sample in Skin.Samples.GetAvailableResources())
                {
                    foreach (string possiblePrefix in possiblePrefixes)
                    {
                        if (!sample.StartsWith(possiblePrefix, StringComparison.InvariantCultureIgnoreCase))
                            continue;

                        string indexString = Path.GetFileNameWithoutExtension(sample)[possiblePrefix.Length..];
                        if (string.IsNullOrEmpty(indexString))
                            indices.Add(1);
                        if (int.TryParse(indexString, out int index))
                            indices.Add(index);
                    }
                }
            }

            return indices.OrderBy(i => i).Select(i => new SampleSet(i));
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
