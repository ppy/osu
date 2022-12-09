// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class LongestComboCounter : ComboCounter
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colours, ScoreProcessor scoreProcessor)
        {
            Colour = colours.YellowLighter;
            Current.BindTo(scoreProcessor.HighestCombo);
        }

        protected override IHasText CreateText() => new TextComponent();

        private partial class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = $"{value}x";
            }

            private readonly OsuSpriteText text;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 20)
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Font = OsuFont.Numeric.With(size: 8),
                                    Text = @"longest",
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Font = OsuFont.Numeric.With(size: 8),
                                    Text = @"combo",
                                    Padding = new MarginPadding { Bottom = 3f }
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
