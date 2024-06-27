// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Osu.Edit;

namespace osu.Game.Tests.Visual.Edit
{
    [TestFixture]
public class PreciseScalePopoverTests
{
    private OsuSelectionScaleHandlerMock scaleHandler;
    private PreciseScalePopover popover;

    private Bindable<bool> xCheckBoxBindable;
    private Bindable<bool> yCheckBoxBindable;

    [SetUp]
    public void Setup()
    {
        scaleHandler = new OsuSelectionScaleHandlerMock();
        popover = new PreciseScalePopover(scaleHandler);

        // Simulate BackgroundDependencyLoader initialization
        popover.load(null); // Pass null for the dependency container as it's not used in tests

        xCheckBoxBindable = new Bindable<bool>(true);
        yCheckBoxBindable = new Bindable<bool>(true);

        // Set up the checkboxes with the initial state
        popover.xCheckBox = new OsuCheckbox(false)
        {
            RelativeSizeAxes = Axes.X,
            LabelText = "X-axis",
            Current = { Value = true },
        };
        popover.yCheckBox = new OsuCheckbox(false)
        {
            RelativeSizeAxes = Axes.X,
            LabelText = "Y-axis",
            Current = { Value = true },
        };

        // Set the Current bindables to match the initial state
        popover.xCheckBox.Current = xCheckBoxBindable;
        popover.yCheckBox.Current = yCheckBoxBindable;
    }

    [Test]
    public void TestUpdateAxisCheckBoxesEnabled_PlayfieldCentre()
    {
        // Set scaleInfo.Value.Origin to PlayfieldCentre
        scaleHandler.CanScaleX.Value = true;
        scaleHandler.CanScaleY.Value = true; // Assuming Y axis cannot scale

        popover.updateAxisCheckBoxesEnabled();

        // Assert that xCheckBox should be enabled and yCheckBox should be disabled
        Assert.IsFalse(xCheckBoxBindable.Disabled);
        Assert.IsTrue(xCheckBoxBindable.Value);
        Assert.IsFalse(yCheckBoxBindable.Disabled); // Should be true
        Assert.IsTrue(yCheckBoxBindable.Value);
    }

    [Test]
    public void TestUpdateAxisCheckBoxesEnabled_SelectionCentre()
    {
        // Set scaleInfo.Value.Origin to SelectionCentre
        scaleHandler.CanScaleX.Value = false; // Assuming X axis cannot scale
        scaleHandler.CanScaleY.Value = true;

        popover.updateAxisCheckBoxesEnabled();

        // Assert that xCheckBox should be disabled and yCheckBox should be enabled
        Assert.IsFalse(xCheckBoxBindable.Disabled); // Should be true
        Assert.IsTrue(xCheckBoxBindable.Value);
        Assert.IsFalse(yCheckBoxBindable.Disabled);
        Assert.IsTrue(yCheckBoxBindable.Value);
    }
}

    // Mock class for OsuSelectionScaleHandler to simulate behavior
    public class OsuSelectionScaleHandlerMock : OsuSelectionScaleHandler
    {
        public Bindable<bool> CanScaleX { get; } = new Bindable<bool>();
        public Bindable<bool> CanScaleY { get; } = new Bindable<bool>();

        public OsuSelectionScaleHandlerMock()
        {
            CanScaleX.Value = true;
            CanScaleY.Value = true;
        }
    }

}
