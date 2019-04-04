// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Humanizer;
using Newtonsoft.Json.Serialization;

namespace osu.Game.IO.Serialization
{
    public class KeyContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.Underscore();
        }
    }
}
