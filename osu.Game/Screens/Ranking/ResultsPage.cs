// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking
{
    public abstract class ResultsPage : Container
    {
        protected readonly ScoreInfo Score;
        protected readonly WorkingBeatmap Beatmap;
        private CircularContainer content;
        private Box fill;

        protected override Container<Drawable> Content => content;

        protected ResultsPage(ScoreInfo score, WorkingBeatmap beatmap)
        {
            Score = score;
            Beatmap = beatmap;
            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            fill.Delay(400).FadeInFromZero(600);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            AddRangeInternal(new Drawable[]
            {
                fill = new Box
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Colour = colours.Gray6
                },
                new CircularContainer
                {
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = colours.GrayF.Opacity(0.8f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 1,
                    },
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 20,
                    BorderColour = Color4.White,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        },
                    }
                },
                content = new CircularContainer
                {
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.2f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 15,
                    },
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Size = new Vector2(0.88f),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            });
        }
    }
}
