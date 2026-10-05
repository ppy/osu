// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Volume;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneVolumeOverlay : OsuTestScene
    {
        private VolumeOverlay volume = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
            dependencies.CacheAs(volume = new VolumeOverlay());
            return dependencies;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddRange(new Drawable[]
            {
                volume,
                new GlobalScrollAdjustsVolume
                {
                    RelativeSizeAxes = Axes.Both,
                },
            });

            AddStep("show controls", () => volume.Show());
        }
    }
}
