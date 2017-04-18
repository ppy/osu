// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// A drawable object which visualises the hit result of a <see cref="Judgements.Judgement"/>.
    /// </summary>
    /// <typeparam name="TJudgement">The type of judgement to visualise.</typeparam>
    public class DrawableJudgement<TJudgement> : Container
        where TJudgement : Judgement
    {
        protected readonly TJudgement Judgement;

        protected readonly SpriteText JudgementText;

        /// <summary>
        /// Creates a drawable which visualises a <see cref="Judgements.Judgement"/>.
        /// </summary>
        /// <param name="judgement">The judgement to visualise.</param>
        public DrawableJudgement(TJudgement judgement)
        {
            Judgement = judgement;

            AutoSizeAxes = Axes.Both;

            string resultString = judgement.Result == HitResult.Hit ? judgement.ResultString : judgement.Result.GetDescription();

            Children = new[]
            {
                JudgementText = new OsuSpriteText
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Text = resultString.ToUpper(),
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

                    Delay(100);
                    FadeOut(400);
                    break;
            }

            Expire();
        }
    }
}
