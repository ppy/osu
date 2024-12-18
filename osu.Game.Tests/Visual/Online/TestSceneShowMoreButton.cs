// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Allocation;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneShowMoreButton : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Green);

        public TestSceneShowMoreButton()
        {
            ShowMoreButton button = null;

            int fireCount = 0;

            Add(button = new ShowMoreButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Action = () =>
                {
                    fireCount++;
                    // ReSharper disable once AccessToModifiedClosure
                    // ReSharper disable once PossibleNullReferenceException
                    Scheduler.AddDelayed(() => button.IsLoading = false, 2000);
                }
            });

            AddStep("click button", () => button.TriggerClick());

            AddAssert("action fired once", () => fireCount == 1);
            AddAssert("is in loading state", () => button.IsLoading);

            AddStep("click button", () => button.TriggerClick());

            AddAssert("action not fired", () => fireCount == 1);
            AddAssert("is in loading state", () => button.IsLoading);

            AddUntilStep("wait for loaded", () => !button.IsLoading);

            AddStep("click button", () => button.TriggerClick());

            AddAssert("action fired twice", () => fireCount == 2);
            AddAssert("is in loading state", () => button.IsLoading);
        }
    }
}
