// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using osu.Game.Database;
using osu.Game.IO.Serialization;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Changelog
{
    [Serializable]
    public class ReleaseStreamInfo : IJsonSerializable
    {
        public string Name;
        public string DisplayVersion;

        public float Users;

        public bool IsFeatured;
    }
}
