// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Timing;
using osu.Game.Tests.Visual.UserInterface;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneFormDiscreteAdjustmentControl : ThemeComparisonTestScene
    {
        [Cached]
        private EditorBeatmap editorBeatmap { get; set; }

        public TestSceneFormDiscreteAdjustmentControl()
            : base(false)
        {
            editorBeatmap = new EditorBeatmap(CreateBeatmap(new OsuRuleset().RulesetInfo));
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Child = new FormDiscreteAdjustmentControl<double>(0.05)
            {
                Caption = "Slider velocity",
                Current = new BindableDouble(1)
                {
                    MinValue = 0.05,
                    MaxValue = 10,
                    Precision = 0.01,
                },
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 0.2f,
            }
        };

        [Test]
        public void TestBehaviour()
        {
            AddStep("create content", () => CreateThemedContent(OverlayColourScheme.Aquamarine));
            AddStep("click control", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<OsuTextBox>().Single());
                InputManager.Click(MouseButton.Left);
            });
            AddStep("input 4", () => this.ChildrenOfType<OsuTextBox>().Single().Text = "4");
            AddStep("commit", () => InputManager.Key(Key.Enter));
            AddAssert("current is 4", () => this.ChildrenOfType<FormDiscreteAdjustmentControl<double>>().Single().Current.Value, () => Is.EqualTo(4));

            AddStep("decrement by smallest step", () =>
            {
                var control = this.ChildrenOfType<DiscreteAdjustmentControl<double>>().Single();
                InputManager.MoveMouseTo(control.ScreenSpaceDrawQuad.Centre - new Vector2(5, 0));
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("current is 3.95", () => this.ChildrenOfType<FormDiscreteAdjustmentControl<double>>().Single().Current.Value, () => Is.EqualTo(3.95));

            AddStep("increment by biggest step", () =>
            {
                var control = this.ChildrenOfType<DiscreteAdjustmentControl<double>>().Single();
                InputManager.MoveMouseTo(control.ScreenSpaceDrawQuad.TopRight + new Vector2(-5, 5));
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("current is 4.45", () => this.ChildrenOfType<FormDiscreteAdjustmentControl<double>>().Single().Current.Value, () => Is.EqualTo(4.45));

            AddStep("start increment by biggest step", () =>
            {
                var control = this.ChildrenOfType<DiscreteAdjustmentControl<double>>().Single();
                InputManager.MoveMouseTo(control.ScreenSpaceDrawQuad.TopRight + new Vector2(-5, 5));
                InputManager.PressButton(MouseButton.Left);
            });
            AddUntilStep("current reached max", () => this.ChildrenOfType<FormDiscreteAdjustmentControl<double>>().Single().Current.Value, () => Is.EqualTo(10));
            AddStep("release mouse", () => InputManager.ReleaseButton(MouseButton.Left));

            AddStep("change current externally", () => this.ChildrenOfType<FormDiscreteAdjustmentControl<double>>().Single().Current.Value = 4);
            AddAssert("text box says 4", () => this.ChildrenOfType<OsuTextBox>().Single().Text, () => Is.EqualTo("4"));
        }
    }
}
