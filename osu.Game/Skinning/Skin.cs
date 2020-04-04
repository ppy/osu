// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;

namespace osu.Game.Skinning
{
    public abstract class Skin : IDisposable, ISkin
    {
        public readonly SkinInfo SkinInfo;

        public SkinConfiguration Configuration { get; protected set; }

        public abstract Drawable GetDrawableComponent(ISkinComponent componentName);

        public abstract SampleChannel GetSample(ISampleInfo sampleInfo);

        public abstract Texture GetTexture(string componentName);

        public abstract IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup);

        protected Skin(SkinInfo skin)
        {
            SkinInfo = skin;
        }

        #region Disposal

        ~Skin()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;
        }

        #endregion
    }
}
