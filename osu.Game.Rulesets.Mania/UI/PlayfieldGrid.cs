// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class PlayfieldGrid : GridContainer, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }
    }
}
