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
        [SettingSource("ScaleX", "The horizontal scale at which this component should be displayed.")]
        public BindableNumber<float> SkinScaleX { get; } = new BindableFloat(1);

        [SettingSource("ScaleY", "The vertical scale at which this component should be displayed.")]
        public BindableNumber<float> SkinScaleY { get; } = new BindableFloat(1);

        [SettingSource("PositionX", "The horizontal position at which this component should be displayed.")]
        public BindableNumber<float> SkinPositionX { get; } = new BindableFloat();

        [SettingSource("PositionY", "The vertical position at which this component should be displayed.")]
        public BindableNumber<float> SkinPositionY { get; } = new BindableFloat();

        [SettingSource("Rotation", "The rotation at which this component should be displayed.")]
        public BindableNumber<float> SkinRotation { get; } = new BindableFloat();

        [SettingSource("Anchor", "The screen edge this component should align to.")]
        public Bindable<Anchor> SkinAnchor { get; } = new Bindable<Anchor>();

        protected SkinnableHUDComponent(ISkinComponent component, Func<ISkinComponent, Drawable> defaultImplementation, Func<ISkinSource, bool> allowFallback = null, ConfineMode confineMode = ConfineMode.NoScaling)
            : base(component, defaultImplementation, allowFallback, confineMode)
        {
            SkinScaleX.BindValueChanged(x => Drawable.Scale = new Vector2(x.NewValue, Drawable.Scale.Y));
            SkinScaleY.BindValueChanged(y => Drawable.Scale = new Vector2(Drawable.Scale.X, y.NewValue));

            SkinPositionX.BindValueChanged(x => Position = new Vector2(x.NewValue, Position.Y));
            SkinPositionY.BindValueChanged(y => Position = new Vector2(Position.X, y.NewValue));

            SkinRotation.BindValueChanged(rotation => Drawable.Rotation = rotation.NewValue);
            SkinAnchor.BindValueChanged(anchor =>
            {
                Drawable.Anchor = anchor.NewValue;
            });
        }

        protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
        {
            SkinScaleX.Value = Drawable.Scale.X;
            SkinScaleY.Value = Drawable.Scale.Y;
            SkinPositionX.Value = Position.X;
            SkinPositionY.Value = Position.Y;
            SkinRotation.Value = Drawable.Rotation;
            SkinAnchor.Value = Anchor;
            return base.OnInvalidate(invalidation, source);
        }
    }
}
