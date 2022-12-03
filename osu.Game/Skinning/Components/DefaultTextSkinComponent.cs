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
        public Bindable<DefaultFont> Font { get; } = new Bindable<DefaultFont>(DefaultFont.Torus);

        protected abstract void SetFont(FontUsage font);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Font.BindValueChanged(e =>
            {
                FontUsage f = e.NewValue switch
                {
                    DefaultFont.Venera => OsuFont.Numeric,
                    DefaultFont.Torus => OsuFont.Torus,
                    DefaultFont.TorusAlt => OsuFont.TorusAlternate,
                    DefaultFont.Inter => OsuFont.Inter,
                    _ => OsuFont.Default
                };

                SetFont(f);
            }, true);
        }
    }
}
