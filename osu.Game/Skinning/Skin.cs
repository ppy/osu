// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using OpenTK.Graphics;

namespace osu.Game.Skinning
{
    public abstract class Skin : IDisposable, ISkinSource
    {
        public readonly SkinInfo SkinInfo;

        public virtual SkinConfiguration Configuration { get; protected set; }

        public event Action SourceChanged;

        public abstract Drawable GetDrawableComponent(string componentName);

        public abstract SampleChannel GetSample(string sampleName);

        public abstract Texture GetTexture(string componentName);

        public virtual Color4? GetColour(string colourName)
        {
            var namespaces = colourName.Split('/');

            switch (namespaces[0])
            {
                case "Play":
                    switch (namespaces[1])
                    {
                        case "Combo":
                            int index = int.Parse(namespaces[2]);
                            return Configuration.ComboColours.Count == 0 ? (Color4?)null : Configuration.ComboColours[index % Configuration.ComboColours.Count];
                    }

                    break;
            }

            return null;
        }

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
