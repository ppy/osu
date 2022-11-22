// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Overlays.Mods;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Practice
{
    public class PracticeOverlay : ShearedOverlayContainer
    {
        public PracticeGameplayPreview Preview = null!;

        public PracticeOverlay()
            : base(OverlayColourScheme.Green)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Header.Title = PracticeOverlayStrings.PracticeOverlayHeaderTitle;
            Header.Description = PracticeOverlayStrings.PracticeOverlayHeaderDescription;

            MainAreaContent.Add(Preview = new PracticeGameplayPreview());
        }
    }
}
