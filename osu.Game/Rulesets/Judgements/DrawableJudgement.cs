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
using osu.Game.Skinning;
using OpenTK.Graphics;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// A drawable object which visualises the hit result of a <see cref="Judgements.Judgement"/>.
    /// </summary>
    public class DrawableJudgement : Container
    {
        private const float judgement_size = 80;

        private OsuColour colours;

        protected readonly JudgementResult Result;

        public readonly DrawableHitObject JudgedObject;

        protected SpriteText JudgementText;

        /// <summary>
        /// Creates a drawable which visualises a <see cref="Judgements.Judgement"/>.
        /// </summary>
        /// <param name="result">The judgement to visualise.</param>
        /// <param name="judgedObject">The object which was judged.</param>
        public DrawableJudgement(JudgementResult result, DrawableHitObject judgedObject)
        {
            Result = result;
            JudgedObject = judgedObject;

            Size = new Vector2(judgement_size);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;

            Child = new SkinnableDrawable($"Play/{Result.Type}", _ => JudgementText = new OsuSpriteText
            {
                Text = Result.Type.GetDescription().ToUpperInvariant(),
                Font = @"Venera",
                Colour = judgementColour(Result.Type),
                Scale = new Vector2(0.85f, 1),
                TextSize = 12
            }, restrictSize: false);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.FadeInFromZero(100, Easing.OutQuint);

            var origScale = Scale;

            switch (Result.Type)
            {
                case HitResult.None:
                    break;
                case HitResult.Miss:
                    this.ScaleTo(origScale * 1.6f);
                    this.ScaleTo(origScale, 100, Easing.In);

                    this.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);
                    this.RotateTo(40, 800, Easing.InQuint);

                    this.Delay(600).FadeOut(200);
                    break;
                default:
                    this.ScaleTo(origScale * 0.9f);
                    this.ScaleTo(origScale, 500, Easing.OutElastic);

                    this.Delay(100).FadeOut(400);
                    break;
            }

            Expire(true);
        }

        private Color4 judgementColour(HitResult judgement)
        {
            switch (judgement)
            {
                case HitResult.Perfect:
                case HitResult.Great:
                    return colours.Blue;
                case HitResult.Ok:
                case HitResult.Good:
                    return colours.Green;
                case HitResult.Meh:
                    return colours.Yellow;
                case HitResult.Miss:
                    return colours.Red;
            }

            return Color4.White;
        }
    }
}
