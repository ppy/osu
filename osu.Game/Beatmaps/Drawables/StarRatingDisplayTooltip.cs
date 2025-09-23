// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Beatmaps.Drawables
{
    public partial class StarRatingDisplayTooltip : VisibilityContainer, ITooltip<StarDifficulty>
    {
        private readonly OverlayColourProvider? colourProvider;

        private Container content = null!;

        private StarDifficulty starDifficulty;
        private OsuSpriteText adjustedByModsText = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public StarRatingDisplayTooltip(OverlayColourProvider? colourProvider = null)
        {
            this.colourProvider = colourProvider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider?.Background4 ?? colours.Gray3,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Vertical = 10, Horizontal = 15 },
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                adjustedByModsText = new OsuSpriteText
                                {
                                    Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold),
                                },
                            }
                        },
                    }
                },
            };

            updateDisplay();
        }

        private void updateDisplay()
        {
            if (!Precision.AlmostEquals(starDifficulty.Stars, starDifficulty.NoModStars))
            {
                adjustedByModsText.Text = $"This value is being adjusted by mods ({starDifficulty.NoModStars:0.0#} → {starDifficulty.Stars:0.0#}).";
                content.Show();
            }
            else
                content.Hide();
        }

        public void SetContent(StarDifficulty starDifficulty)
        {
            this.starDifficulty = starDifficulty;
            updateDisplay();
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;
    }
}
