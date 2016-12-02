//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Screens.Ranking
{
    class Results : OsuGameMode
    {
        protected override BackgroundMode CreateBackground() => new BackgroundModeBeatmap(Beatmap);

        private static readonly Vector2 BACKGROUND_BLUR = new Vector2(20);

        ScoreDisplay scoreDisplay;

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);
            Background.Schedule(() => (Background as BackgroundModeBeatmap)?.BlurTo(BACKGROUND_BLUR, 1000));
        }

        protected override bool OnExiting(GameMode next)
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

    class ScoreDisplay : Container
    {
        public ScoreDisplay(Score s)
        {
            AutoSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                new FlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FlowDirection.VerticalOnly,
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            TextSize = 40,
                            Text = $@"Accuracy: {s.Accuracy:#0.00%}",
                        },
                        new SpriteText
                        {
                            TextSize = 40,
                            Text = $@"Score: {s.TotalScore}",
                        },
                        new SpriteText
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
