// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    /// <summary>
    /// A <see cref="StatisticDisplay"/> to display the player's combo.
    /// </summary>
    public partial class ComboStatistic : CounterStatistic
    {
        private readonly bool isPerfect;

        private Drawable perfectText = null!;

        /// <summary>
        /// Creates a new <see cref="ComboStatistic"/>.
        /// </summary>
        /// <param name="combo">The combo to be displayed.</param>
        /// <param name="maxCombo">The maximum value of <paramref name="combo"/>.</param>
        public ComboStatistic(int combo, int? maxCombo)
            : base(BeatmapsetsStrings.ShowScoreboardHeadersCombo, combo, maxCombo)
        {
            isPerfect = combo == maxCombo;
        }

        public override void Appear()
        {
            base.Appear();

            if (isPerfect)
            {
                using (BeginDelayedSequence(AccuracyCircle.ACCURACY_TRANSFORM_DURATION / 2))
                    perfectText.FadeIn(50);
            }
        }

        protected override Drawable CreateContent() => new FillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(10, 0),
            Children = new[]
            {
                base.CreateContent().With(_ =>
                {
                    Anchor = Anchor.CentreLeft;
                    Origin = Anchor.CentreLeft;
                }),
                perfectText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = "PERFECT",
                    Font = OsuFont.Torus.With(size: 11, weight: FontWeight.SemiBold),
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#66FFCC"), Color4Extensions.FromHex("#FF9AD7")),
                    Alpha = 0,
                    UseFullGlyphHeight = false,
                }
            }
        };
    }
}
