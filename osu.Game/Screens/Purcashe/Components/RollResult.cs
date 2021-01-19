// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Screens.Purcashe.Components
{
    public class RollResult
    {
        public ItemPanel.Rank Rank { get; set; }
        public ItemPanel.LevelStats Level { get; set; }
        public int PP { get; set; }
        public string TexturePath { get; set; }
        public string RollName { get; set; }
    }
}
