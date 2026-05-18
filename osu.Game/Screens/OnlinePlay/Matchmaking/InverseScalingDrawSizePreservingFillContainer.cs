// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking
{
    public partial class InverseScalingDrawSizePreservingFillContainer : ScalingContainer.ScalingDrawSizePreservingFillContainer
    {
        public InverseScalingDrawSizePreservingFillContainer()
            : base(true)
        {
        }

        protected override void Update()
        {
            // TODO: Consider whether we still want to be applying OsuGame.ScalingContainerTargetDrawSize here
            // to allow mobile to not become unusable.

            Size = new Vector2(CurrentScale);
            Scale = new Vector2(1 / CurrentScale);
        }
    }
}
