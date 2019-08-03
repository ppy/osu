// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.IO.Stores;

namespace osu.Game.IO
{
    public abstract class OsuCachedOnlineStore : CachedOnlineStore
    {
        protected override string CachePath => Path.Combine(Path.GetTempPath(), "osu");
    }
}
