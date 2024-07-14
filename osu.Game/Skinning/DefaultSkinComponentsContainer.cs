// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which can be used to specify default skin components layouts.
    /// Handles applying a default layout to the components.
    /// </summary>
    public partial class DefaultSkinComponentsContainer : Container
    {
        private readonly Action<Container>? applyDefaults;

        /// <summary>
        /// Construct a wrapper with defaults that should be applied once.
        /// </summary>
        /// <param name="applyDefaults">A function to apply the default layout.</param>
        public DefaultSkinComponentsContainer(Action<Container> applyDefaults)
        {
            RelativeSizeAxes = Axes.Both;

            this.applyDefaults = applyDefaults;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // schedule is required to allow children to run their LoadComplete and take on their correct sizes.
            ScheduleAfterChildren(() => applyDefaults?.Invoke(this));
        }
    }
}
