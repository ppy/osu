// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Skinning
{
    /// <summary>
    /// Provides access to skinnable elements.
    /// </summary>
    public interface ISkinSource
    {
        event Action SourceChanged;

        Drawable GetDrawableComponent(string componentName);

        Texture GetTexture(string componentName);

        SampleChannel GetSample(string sampleName);

        TValue GetValue<TConfiguration, TValue>(Func<TConfiguration, TValue> query) where TConfiguration : SkinConfiguration;
    }
}
