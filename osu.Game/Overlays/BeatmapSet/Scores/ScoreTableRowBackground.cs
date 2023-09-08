// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Scoring;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public partial class ScoreTableRowBackground : CompositeDrawable
    {
        private const int fade_duration = 100;

        private readonly Box hoveredBackground;
        private readonly Box background;

        private readonly int index;
        private readonly ScoreInfo score;

        public ScoreTableRowBackground(int index, ScoreInfo score, float height)
        {
            this.index = index;
            this.score = score;

            RelativeSizeAxes = Axes.X;
            Height = height;

            CornerRadius = 5;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                hoveredBackground = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider, IAPIProvider api)
        {
            bool isOwnScore = api.LocalUser.Value.Id == score.UserID;

            if (isOwnScore)
                background.Colour = colours.GreenDarker;
            else if (index % 2 == 0)
                background.Colour = colourProvider.Background4;
            else
                background.Alpha = 0;

            hoveredBackground.Colour = isOwnScore ? colours.GreenDark : colourProvider.Background3;
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoveredBackground.FadeIn(fade_duration, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoveredBackground.FadeOut(fade_duration, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
