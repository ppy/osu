// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Volume;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneVolumeOverlay : OsuTestScene
    {
        private VolumeOverlay volume;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddRange(new Drawable[]
            {
                volume = new VolumeOverlay(),
                new VolumeControlReceptor
                {
                    RelativeSizeAxes = Axes.Both,
                    ActionRequested = action => volume.Adjust(action),
                    ScrollActionRequested = (action, amount, isPrecise) => volume.Adjust(action, amount, isPrecise),
                },
            });

            AddStep("show controls", () => volume.Show());
        }
    }
}
