// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;
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

        public virtual Color4? GetComboColour(IHasComboIndex comboObject) =>
            Configuration.ComboColours.Count == 0 ? (Color4?)null : Configuration.ComboColours[comboObject.ComboIndex % Configuration.ComboColours.Count];

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
