// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which groups the components of a <see cref="SkinnableTargetContainer"/> into a single object.
    /// Optionally also applies a default layout to the components.
    /// </summary>
    [Serializable]
    public class SkinnableTargetComponentsContainer : Container, ISkinnableDrawable
    {
        public bool IsEditable => false;

        public bool UsesFixedAnchor { get; set; }

        private readonly Action<Container> applyDefaults;

        /// <summary>
        /// Construct a wrapper with defaults that should be applied once.
        /// </summary>
        /// <param name="applyDefaults">A function to apply the default layout.</param>
        public SkinnableTargetComponentsContainer(Action<Container> applyDefaults)
            : this()
        {
            this.applyDefaults = applyDefaults;
        }

        [JsonConstructor]
        public SkinnableTargetComponentsContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // schedule is required to allow children to run their LoadComplete and take on their correct sizes.
            ScheduleAfterChildren(() => applyDefaults?.Invoke(this));
        }
    }
}
