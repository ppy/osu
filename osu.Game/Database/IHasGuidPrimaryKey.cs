// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using Realms;

namespace osu.Game.Database
{
    public interface IHasGuidPrimaryKey
    {
        [JsonIgnore]
        [PrimaryKey]
        Guid ID { get; }
    }
}
