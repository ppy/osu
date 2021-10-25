// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
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
        public event Action BeatmapSkinChanged;

        /// <summary>
        /// The combo colours of this skin.
        /// If empty, the default combo colours will be used.
        /// </summary>
        public readonly BindableList<Colour4> ComboColours;

        private readonly Skin skin;

        public EditorBeatmapSkin(Skin skin)
        {
            this.skin = skin;

            ComboColours = new BindableList<Colour4>();
            if (skin.Configuration.ComboColours != null)
                ComboColours.AddRange(skin.Configuration.ComboColours.Select(c => (Colour4)c));
            ComboColours.BindCollectionChanged((_, __) => updateColours());
        }

        private void invokeSkinChanged() => BeatmapSkinChanged?.Invoke();

        private void updateColours()
        {
            skin.Configuration.CustomComboColours = ComboColours.Select(c => (Color4)c).ToList();
            invokeSkinChanged();
        }

        #region Delegated ISkin implementation

        public Drawable GetDrawableComponent(ISkinComponent component) => skin.GetDrawableComponent(component);
        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT) => skin.GetTexture(componentName, wrapModeS, wrapModeT);
        public ISample GetSample(ISampleInfo sampleInfo) => skin.GetSample(sampleInfo);
        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup) => skin.GetConfig<TLookup, TValue>(lookup);

        #endregion
    }
}
