// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Localisation.SkinComponents;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A skin component that contains text and allows the user to choose its font.
    /// </summary>
    public abstract partial class FontAdjustableSkinComponent : Container, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Font), nameof(SkinnableComponentStrings.FontDescription))]
        public Bindable<Typeface> Font { get; } = new Bindable<Typeface>(Typeface.Torus);

        /// <summary>
        /// Implement to apply the user font selection to one or more components.
        /// </summary>
        protected abstract void SetFont(FontUsage font);

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Font.BindValueChanged(e =>
            {
                // We only have bold weight for venera, so let's force that.
                FontWeight fontWeight = e.NewValue == Typeface.Venera ? FontWeight.Bold : FontWeight.Regular;

                FontUsage f = OsuFont.GetFont(e.NewValue, weight: fontWeight);
                SetFont(f);
            }, true);
        }
    }
}
