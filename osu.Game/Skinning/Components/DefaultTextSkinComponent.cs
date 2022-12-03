// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;

namespace osu.Game.Skinning.Components
{
    /// <summary>
    /// Skin element that contains text and have ability to control its font.
    /// </summary>
    public abstract partial class DefaultTextSkinComponent : Container, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Font", "Font to use.")]
        public Bindable<Typeface> Font { get; } = new Bindable<Typeface>(Typeface.Torus);

        protected abstract void SetFont(FontUsage font);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Font.BindValueChanged(e =>
            {
                FontUsage f = OsuFont.GetFont(e.NewValue);
                SetFont(f);
            }, true);
        }
    }
}
