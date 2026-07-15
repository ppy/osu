// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Ranking.Statistics
{
    public partial class StatisticItemHeader : CompositeDrawable, IHasText
    {
        public LocalisableString Text
        {
            get => text;
            set
            {
                if (text == value) return;

                text = value;
                if (IsLoaded)
                    spriteText.Text = value;
            }
        }

        private LocalisableString text;
        private OsuSpriteText spriteText = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new Container
            {
                AutoSizeAxes = Axes.Both,
                Margin = new MarginPadding
                {
                    Horizontal = 10,
                    Top = 5,
                    Bottom = 20,
                },
                Children = new Drawable[]
                {
                    spriteText = new OsuSpriteText
                    {
                        Text = text,
                        Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
                    },
                    new Box
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.TopCentre,
                        Margin = new MarginPadding { Top = 4 },
                        RelativeSizeAxes = Axes.X,
                        Height = 2,
                        Colour = Color4Extensions.FromHex("#66FFCC")
                    }
                }
            };
        }
    }
}
