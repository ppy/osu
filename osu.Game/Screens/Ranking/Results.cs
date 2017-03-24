// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes.Scoring;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Ranking
{
    internal class Results : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        private static readonly Vector2 background_blur = new Vector2(20);

        private ScoreDisplay scoreDisplay;

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Background.Schedule(() => (Background as BackgroundScreenBeatmap)?.BlurTo(background_blur, 1000));
        }

        protected override bool OnExiting(Screen next)
        {
            Background.Schedule(() => Background.FadeColour(Color4.White, 500));
            return base.OnExiting(next);
        }

        public Score Score
        {
            set
            {
                scoreDisplay?.FadeOut(500);
                scoreDisplay?.Expire();

                scoreDisplay = new ScoreDisplay(value)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                };

                Add(scoreDisplay);

                scoreDisplay.FadeIn(500);
                scoreDisplay.ScaleTo(0.1f);
                scoreDisplay.ScaleTo(1, 1000, EasingTypes.OutElastic);
                scoreDisplay.RotateTo(360 * 5, 1000, EasingTypes.OutElastic);

            }
        }
    }

    internal class ScoreDisplay : Container
    {
        public ScoreDisplay(Score s)
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            TextSize = 40,
                            Text = $@"Accuracy: {s.Accuracy:#0.00%}",
                        },
                        new OsuSpriteText
                        {
                            TextSize = 40,
                            Text = $@"Score: {s.TotalScore}",
                        },
                        new OsuSpriteText
                        {
                            TextSize = 40,
                            Text = $@"MaxCombo: {s.MaxCombo}",
                        }
                    }
                }
            };
        }
    }
}
