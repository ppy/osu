//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System.Resources;
using osu.Framework.Resources;

namespace osu.Framework.Graphics.Textures
{
    public class PrefixTextureStore : TextureStore
    {
        string Prefix;

        public PrefixTextureStore(string prefix, IResourceStore<byte[]> stores) : base(stores)
        {
            Prefix = prefix;
        }

        public override Texture Get(string name)

        {
            return base.Get($@"{Prefix}-{name}");
        }
    }
}
