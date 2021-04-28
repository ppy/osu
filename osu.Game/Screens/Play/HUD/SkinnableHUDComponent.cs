// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Layout;
using osu.Game.Configuration;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    /// <summary>
    /// A skinnable HUD component which can be scaled and repositioned at a skinner/user's will.
    /// </summary>
    public abstract class SkinnableHUDComponent : SkinnableDrawable
    {
        [SettingSource("Scale", "The scale at which this component should be displayed.")]
        public BindableNumber<float> SkinScale { get; } = new BindableFloat(1);

        [SettingSource("Position", "The position at which this component should be displayed.")]
        public BindableNumber<float> SkinPosition { get; } = new BindableFloat();

        [SettingSource("Rotation", "The rotation at which this component should be displayed.")]
        public BindableNumber<float> SkinRotation { get; } = new BindableFloat();

        [SettingSource("Anchor", "The screen edge this component should align to.")]
        public Bindable<Anchor> SkinAnchor { get; } = new Bindable<Anchor>();

        protected SkinnableHUDComponent(ISkinComponent component, Func<ISkinComponent, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.NoScaling)
            : base(component, defaultImplementation, allowFallback, confineMode)
        {
            SkinScale.BindValueChanged(scale => Drawable.Scale = new Vector2(scale.NewValue));
            SkinPosition.BindValueChanged(position => Position = new Vector2(position.NewValue));
            SkinRotation.BindValueChanged(rotation => Drawable.Rotation = rotation.NewValue);
            SkinAnchor.BindValueChanged(anchor =>
            {
                Drawable.Anchor = anchor.NewValue;
            });
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            SkinScale.Value = Drawable.Scale.X;
            SkinPosition.Value = Position.X;
            SkinRotation.Value = Drawable.Rotation;
            SkinAnchor.Value = Anchor;
            return base.OnInvalidate(invalidation, source);
        }
    }
}
