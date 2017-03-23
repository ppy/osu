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

        protected double HitVisibleLength => 600;

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

            FadeInFromZero(100, EasingTypes.OutQuint);

            switch (Judgement.Result)
            {
                case HitResult.Miss:
                    ScaleTo(1.6f);
                    ScaleTo(1, 100, EasingTypes.In);

                    MoveToOffset(new Vector2(0, 100), 800, EasingTypes.InQuint);
                    RotateTo(40, 800, EasingTypes.InQuint);

                    Delay(600);
                    FadeOut(200);
                    break;
                case HitResult.Hit:
                    ScaleTo(0.9f);
                    ScaleTo(1, 500, EasingTypes.OutElastic);

                    Delay(250);
                    FadeOut(250, EasingTypes.OutQuint);
                    break;
            }

            Expire();
        }
    }
}
