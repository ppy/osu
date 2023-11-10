// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Extensions;

namespace osu.Game.Skinning
{
    public static class SerialisableDrawableExtensions
    {
        public static SerialisedDrawableInfo CreateSerialisedInfo(this Drawable component) => new SerialisedDrawableInfo(component);

        public static void ApplySerialisedInfo(this Drawable component, SerialisedDrawableInfo drawableInfo)
        {
            // todo: can probably make this better via deserialisation directly using a common interface.
            component.Position = drawableInfo.Position;
            component.Rotation = drawableInfo.Rotation;
            if (drawableInfo.Width is float width && width != 0 && (component as CompositeDrawable)?.AutoSizeAxes.HasFlagFast(Axes.X) == false)
                component.Width = width;
            if (drawableInfo.Height is float height && height != 0 && (component as CompositeDrawable)?.AutoSizeAxes.HasFlagFast(Axes.Y) == false)
                component.Height = height;
            component.Scale = drawableInfo.Scale;
            component.Anchor = drawableInfo.Anchor;
            component.Origin = drawableInfo.Origin;

            if (component is ISerialisableDrawable serialisableDrawable)
            {
                serialisableDrawable.UsesFixedAnchor = drawableInfo.UsesFixedAnchor;

                foreach (var (_, property) in component.GetSettingsSourceProperties())
                {
                    var bindable = ((IBindable)property.GetValue(component)!);

                    if (!drawableInfo.Settings.TryGetValue(property.Name.ToSnakeCase(), out object? settingValue))
                    {
                        // TODO: We probably want to restore default if not included in serialisation information.
                        // This is not simple to do as SetDefault() is only found in the typed Bindable<T> interface right now.
                        continue;
                    }

                    serialisableDrawable.CopyAdjustedSetting(bindable, settingValue);
                }
            }

            if (component is Container container)
            {
                foreach (var child in drawableInfo.Children)
                    container.Add(child.CreateInstance());
            }
        }
    }
}
