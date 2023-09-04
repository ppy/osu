// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json.Serialization;
using osu.Game.Extensions;

namespace osu.Game.IO.Serialization
{
    public class SnakeCaseKeyContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToSnakeCase();
        }
    }
}
