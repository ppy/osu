// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;

namespace osu.Game.Graphics.Textures
{
    /// <summary>
    /// A texture store that bypasses atlasing.
    /// </summary>
    public class LargeTextureStore : TextureStore
    {
        public LargeTextureStore(IResourceStore<RawTexture> store = null) : base(store, false)
        {
        }
    }
}
