// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Skinning
{
    /// <summary>
    /// A container which is serialised and can encapsulate multiple skinnable elements into a single return type (for consumption via <see cref="ISkin.GetDrawableComponent"/>.
    /// Will also optionally apply default cross-element layout dependencies when initialised from a non-deserialised source.
    /// </summary>
    [Serializable]
    public class SkinnableTargetWrapper : Container, ISkinnableComponent
    {
        public bool IsEditable => false;

        private readonly Action<Container> applyDefaults;

        /// <summary>
        /// Construct a wrapper with defaults that should be applied once.
        /// </summary>
        /// <param name="applyDefaults">A function to apply the default layout.</param>
        public SkinnableTargetWrapper(Action<Container> applyDefaults)
            : this()
        {
            this.applyDefaults = applyDefaults;
        }

        [JsonConstructor]
        public SkinnableTargetWrapper()
        {
            RelativeSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // schedule is required to allow children to run their LoadComplete and take on their correct sizes.
            Schedule(() => applyDefaults?.Invoke(this));
        }
    }
}
