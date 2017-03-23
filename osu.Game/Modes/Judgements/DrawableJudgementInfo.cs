// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Judgements
{
    /// <summary>
    /// A drawable object which visualises the hit result of a <see cref="JudgementInfo"/>.
    /// </summary>
    /// <typeparam name="TJudgement">The type of judgement to visualise.</typeparam>
    public class DrawableJudgementInfo<TJudgement> : Container
        where TJudgement : JudgementInfo
    {
        protected readonly TJudgement Judgement;

        protected readonly SpriteText JudgementText;

        /// <summary>
        /// Creates a drawable which visualises a <see cref="JudgementInfo"/>.
        /// </summary>
        /// <param name="judgement">The judgement to visualise.</param>
        public DrawableJudgementInfo(TJudgement judgement)
        {
            Judgement = judgement;

            AutoSizeAxes = Axes.Both;

            string scoreString = judgement.Result == HitResult.Hit ? judgement.ScoreString : judgement.Result.GetDescription();

            Children = new[]
            {
                JudgementText = new OsuSpriteText
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Text = scoreString.ToUpper(),
                    Font = @"Venera",
                    TextSize = 16
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            switch (Judgement.Result)
            {
                case HitResult.Miss:
                    Colour = colours.Red;
                    break;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Judgement.Result == HitResult.Miss)
            {
                FadeInFromZero(60);

                ScaleTo(1.6f);
                ScaleTo(1, 100, EasingTypes.In);

                MoveToOffset(new Vector2(0, 100), 800, EasingTypes.InQuint);
                RotateTo(40, 800, EasingTypes.InQuint);

                Delay(600);
                FadeOut(200);
            }

            Expire();
        }
    }
}
