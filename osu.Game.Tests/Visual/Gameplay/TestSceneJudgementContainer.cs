// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneJudgementContainer : OsuTestScene
    {
        private JudgementContainer<DrawableOsuJudgement> judgementContainer = null!;

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("create judgement container", () => Child = judgementContainer = new JudgementContainer<DrawableOsuJudgement>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        [Test]
        public void TestJudgementFromSameHitObjectIsRemoved()
        {
            DrawableHitCircle drawableHitCircle1 = null!;
            DrawableHitCircle drawableHitCircle2 = null!;

            AddStep("create hit circles", () =>
            {
                Add(drawableHitCircle1 = new DrawableHitCircle(createHitCircle()));
                Add(drawableHitCircle2 = new DrawableHitCircle(createHitCircle()));
            });

            int judgementCount = 0;

            AddStep("judge the same hitobject twice via different drawables", () =>
            {
                addDrawableJudgement(drawableHitCircle1);
                drawableHitCircle2.Apply(drawableHitCircle1.HitObject);
                addDrawableJudgement(drawableHitCircle2);
                judgementCount = judgementContainer.Count;
            });

            AddAssert("one judgement in container", () => judgementCount, () => Is.EqualTo(1));
        }

        [Test]
        public void TestJudgementFromDifferentHitObjectIsNotRemoved()
        {
            DrawableHitCircle drawableHitCircle = null!;

            AddStep("create hit circle", () => Add(drawableHitCircle = new DrawableHitCircle(createHitCircle())));

            int judgementCount = 0;

            AddStep("judge two hitobjects via the same drawable", () =>
            {
                addDrawableJudgement(drawableHitCircle);
                drawableHitCircle.Apply(createHitCircle());
                addDrawableJudgement(drawableHitCircle);
                judgementCount = judgementContainer.Count;
            });

            AddAssert("two judgements in container", () => judgementCount, () => Is.EqualTo(2));
        }

        private void addDrawableJudgement(DrawableHitObject drawableHitObject)
        {
            var judgement = new DrawableOsuJudgement();

            judgement.Apply(new JudgementResult(drawableHitObject.HitObject, new OsuJudgement())
            {
                Type = HitResult.Great,
                TimeOffset = Time.Current
            }, drawableHitObject);

            judgementContainer.Add(judgement);
        }

        private HitCircle createHitCircle()
        {
            var circle = new HitCircle();
            circle.ApplyDefaults(new ControlPointInfo(), new BeatmapDifficulty());
            return circle;
        }
    }
}
