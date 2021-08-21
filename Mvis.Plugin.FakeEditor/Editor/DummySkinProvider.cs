using System;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Skinning;

namespace Mvis.Plugin.FakeEditor.Editor
{
    public class DummySkinProvider : ISkin
    {
        public Drawable GetDrawableComponent(ISkinComponent component)
        {
            throw new NotImplementedException();
        }

        public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
        {
            throw new NotImplementedException();
        }

        public ISample GetSample(ISampleInfo sampleInfo)
        {
            throw new NotImplementedException();
        }

        public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
        {
            throw new NotImplementedException();
        }
    }
}
