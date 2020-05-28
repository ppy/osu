// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Holds the path to locate the osu! stable cutting-edge installation.
    /// </summary>
    [Serializable]
    public class StableInfo
    {
        public Bindable<string> StablePath = new Bindable<string>(string.Empty);
    }
}
