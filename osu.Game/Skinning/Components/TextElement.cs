// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation.SkinComponents;

namespace osu.Game.Skinning.Components
{
    [UsedImplicitly]
    public partial class TextElement : FontAdjustableSkinComponent
    {
        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.TextElementText), nameof(SkinnableComponentStrings.TextElementTextDescription))]
        public Bindable<string> Text { get; } = new Bindable<string>("Circles!");

        private readonly OsuSpriteText text;

        public TextElement()
        {
            AutoSizeAxes = Axes.Both;
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

        protected override void SetFont(FontUsage font) => text.Font = font.With(size: 40);
    }
}
