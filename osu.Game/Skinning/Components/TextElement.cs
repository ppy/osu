// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class TextElement : Container, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [SettingSource("Text", "The text to be displayed.")]
        public Bindable<string> Text { get; } = new Bindable<string>("Circles!");

        public TextElement()
        {
            AutoSizeAxes = Axes.Both;
            OsuSpriteText text;
            InternalChildren = new Drawable[]
            {
                text = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = OsuFont.Default.With(size: 40)
                }
            };
            text.Current.BindTo(Text);
        }
    }
}
