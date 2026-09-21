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
            // We may want this container to apply scale still, just at a multiplier
            // of the original scale. Basically in a system like ranked play, we decide
            // what the max UI scale to be supported is, then adjust the inverse
            // container's ctor to stay within the appropriate range.
            //
            // Will become more important when we have mobile releases live and it is more
            // of an immediate concern.

            Size = new Vector2(CurrentScale);
            Scale = new Vector2(1 / CurrentScale);
        }
    }
}
