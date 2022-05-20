// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Timing;
using osuTK;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public class TestSceneTapTimingControl : EditorClockTestScene
    {
        [Cached(typeof(EditorBeatmap))]
        [Cached(typeof(IBeatSnapProvider))]
        private readonly EditorBeatmap editorBeatmap;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Cached]
        private Bindable<ControlPointGroup> selectedGroup = new Bindable<ControlPointGroup>();

        private TapTimingControl control;

        public TestSceneTapTimingControl()
        {
            editorBeatmap = new EditorBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo));
            selectedGroup.Value = editorBeatmap.ControlPointInfo.Groups.First();
        }

        [Test]
        public void TestTapThenReset()
        {
            AddStep("click tap button", () =>
            {
                control.ChildrenOfType<RoundedButton>()
                       .First(b => b.Text == "Tap to beat")
                       .TriggerClick();
            });

            AddUntilStep("wait for track playing", () => Clock.IsRunning);

            AddStep("click reset button", () =>
            {
                control.ChildrenOfType<RoundedButton>()
                       .First(b => b.Text == "Reset")
                       .TriggerClick();
            });

            AddUntilStep("wait for track stopped", () => !Clock.IsRunning);
        }

        [Test]
        public void TestBasic()
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.Value = CreateWorkingBeatmap(editorBeatmap.PlayableBeatmap);
            Beatmap.Disabled = true;

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Width = 400,
                Scale = new Vector2(1.5f),
                Child = control = new TapTimingControl(),
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            Beatmap.Disabled = false;
            base.Dispose(isDisposing);
        }
    }
}
