// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Skinning.Argon;
using osu.Game.Rulesets.Mania.Skinning.Legacy;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Tests.Skinning
{
    public partial class TestSceneComboCounter : ManiaSkinnableTestScene
    {
        [Cached]
        private ScoreProcessor scoreProcessor = new ScoreProcessor(new ManiaRuleset());

        [Test]
        public void TestDisplay()
        {
            setup(Anchor.Centre);
            AddRepeatStep("perform hit", () => scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Great }), 20);
            AddStep("perform miss", () => scoreProcessor.ApplyResult(new JudgementResult(new HitObject(), new Judgement()) { Type = HitResult.Miss }));
        }

        [Test]
        public void TestAnchorOrigin()
        {
            AddStep("set direction down", () => ScrollingInfo.Direction.Value = ScrollingDirection.Down);
            setup(Anchor.TopCentre, 20);
            AddStep("set direction up", () => ScrollingInfo.Direction.Value = ScrollingDirection.Up);
            check(Anchor.BottomCentre, -20);

            AddStep("set direction up", () => ScrollingInfo.Direction.Value = ScrollingDirection.Up);
            setup(Anchor.BottomCentre, -20);
            AddStep("set direction down", () => ScrollingInfo.Direction.Value = ScrollingDirection.Down);
            check(Anchor.TopCentre, 20);

            AddStep("set direction down", () => ScrollingInfo.Direction.Value = ScrollingDirection.Down);
            setup(Anchor.Centre, 20);
            AddStep("set direction up", () => ScrollingInfo.Direction.Value = ScrollingDirection.Up);
            check(Anchor.Centre, 20);

            AddStep("set direction up", () => ScrollingInfo.Direction.Value = ScrollingDirection.Up);
            setup(Anchor.Centre, -20);
            AddStep("set direction down", () => ScrollingInfo.Direction.Value = ScrollingDirection.Down);
            check(Anchor.Centre, -20);
        }

        private void setup(Anchor anchor, float y = 0)
        {
            AddStep($"setup {anchor} {y}", () => SetContents(s =>
            {
                var container = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                };

                if (s is ArgonSkin)
                    container.Add(new ArgonManiaComboCounter());
                else if (s is LegacySkin)
                    container.Add(new LegacyManiaComboCounter());
                else
                    container.Add(new LegacyManiaComboCounter());

                container.Child.Anchor = anchor;
                container.Child.Origin = Anchor.Centre;
                container.Child.Y = y;

                return container;
            }));
        }

        private void check(Anchor anchor, float y)
        {
            AddAssert($"check {anchor} {y}", () =>
            {
                foreach (var combo in this.ChildrenOfType<ISerialisableDrawable>())
                {
                    var drawableCombo = (Drawable)combo;
                    if (drawableCombo.Anchor != anchor || drawableCombo.Y != y)
                        return false;
                }

                return true;
            });
        }
    }
}
