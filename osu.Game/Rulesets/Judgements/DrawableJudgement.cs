// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// A drawable object which visualises the hit result of a <see cref="Judgements.Judgement"/>.
    /// </summary>
    public class DrawableJudgement : Container
    {
        protected readonly Judgement Judgement;

        public readonly DrawableHitObject JudgedObject;

        protected readonly SpriteText JudgementText;

        /// <summary>
        /// Creates a drawable which visualises a <see cref="Judgements.Judgement"/>.
        /// </summary>
        /// <param name="judgement">The judgement to visualise.</param>
        public DrawableJudgement(Judgement judgement, DrawableHitObject judgedObject)
        {
            Judgement = judgement;
            JudgedObject = judgedObject;

            AutoSizeAxes = Axes.Both;

            Children = new[]
            {
                JudgementText = new OsuSpriteText
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Text = judgement.Result.GetDescription().ToUpper(),
                    Font = @"Venera",
                    Scale = new Vector2(0.85f, 1),
                    TextSize = 12
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

            this.FadeInFromZero(100, Easing.OutQuint);

            switch (Judgement.Result)
            {
                case HitResult.None:
                    break;
                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    this.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);
                    this.RotateTo(40, 800, Easing.InQuint);

                    this.Delay(600).FadeOut(200);
                    break;
                default:
                    this.ScaleTo(0.9f);
                    this.ScaleTo(1, 500, Easing.OutElastic);

                    this.Delay(100).FadeOut(400);
                    break;
            }

            Expire(true);
        }
    }
}
