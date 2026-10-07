// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace osu.Game.Online.RankedPlay
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RankedPlayDivision
    {
        I = 1,
        II = 2,
        III = 3,
    }
}
