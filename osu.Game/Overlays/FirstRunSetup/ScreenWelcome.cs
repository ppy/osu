// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Localisation;

namespace osu.Game.Overlays.FirstRunSetup
{
    [LocalisableDescription(typeof(FirstRunSetupOverlayStrings), nameof(FirstRunSetupOverlayStrings.WelcomeTitle))]
    public class ScreenWelcome : FirstRunSetupScreen
    {
        [BackgroundDependencyLoader]
        private void load()
        {
            Content.Children = new Drawable[]
            {
                new OsuTextFlowContainer(cp => cp.Font = OsuFont.Default.With(size: CONTENT_FONT_SIZE))
                {
                    Text = FirstRunSetupOverlayStrings.WelcomeDescription,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                },
            };
        }
    }
}
