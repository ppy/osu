// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A skinnable HUD component which can be scaled and repositioned at a skinner/user's will.
    /// </summary>
    public abstract class SkinnableHUDComponent : SkinnableDrawable
    {
        [SettingSource("Scale", "The scale at which this component should be displayed.")]
        public BindableNumber<float> SkinScale { get; } = new BindableFloat(1)
        {
            Precision = 0.1f,
            MinValue = 0.1f,
            MaxValue = 10,
            Default = 1,
            Value = 1,
        };

        protected SkinnableHUDComponent(ISkinComponent component, Func<ISkinComponent, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.NoScaling)
            : base(component, defaultImplementation, allowFallback, confineMode)
        {
        }
    }
}
