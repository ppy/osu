// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace osu.Game.IO.Serialization.Converters
{
    public class SnakeCaseStringEnumConverter : StringEnumConverter
    {
        public SnakeCaseStringEnumConverter()
        {
            NamingStrategy = new SnakeCaseNamingStrategy();
        }
    }
}
