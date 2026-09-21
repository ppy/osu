// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Timing;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.UI;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneRulesetInputHoverEvents : OsuManualInputManagerTestScene
    {
        [TestCase(true)]
        [TestCase(false)]
        public void TestHoverHandlingIgnoresParent(bool handleHover)
        {
            AddStep("setup", () =>
            {
                var beatmap = new OsuBeatmap
                {
                    HitObjects =
                    [
                        new HitCircle
                        {
                            Position = new Vector2(256, 192)
                        }
                    ]
                };

                foreach (var h in beatmap.HitObjects)
                    h.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

                Child = new TestInputManager
                {
                    HoverEvents = handleHover,
                    UseParentInput = true,
                    Child = new DrawableOsuRuleset(new OsuRuleset(), beatmap)
                    {
                        Clock = new FramedClock(new ManualClock())
                    }
                };
            });

            AddStep("hover hitcircle", () => InputManager.MoveMouseTo(hitCircle));
            AddStep("click", () => osuInputManager.KeyBindingContainer.TriggerPressed(OsuAction.LeftButton));
            AddAssert("hitcircle is hit", () => hitCircle.Result.IsHit);
        }

        private DrawableHitCircle hitCircle => this.ChildrenOfType<DrawableHitCircle>().First();

        private OsuInputManager osuInputManager => this.ChildrenOfType<OsuInputManager>().First();

        private class TestInputManager : PassThroughInputManager
        {
            public bool HoverEvents { get; set; }

            public override bool HandleHoverEvents => HoverEvents;
        }
    }
}
