// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Skinning;
using osu.Framework.Graphics.Containers;


namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class GameplayRankDisplay : Container, ISerialisableDrawable
    {
        public bool UsesFixedAnchor { get; set; }
    }
}
