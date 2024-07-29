// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Dialog;
using osu.Game.Tournament.Models;

namespace osu.Game.Tournament.Screens.Editors.Components
{
    public partial class DeleteBeatmapDialog : DangerousActionDialog
    {
        public DeleteBeatmapDialog(RoundBeatmap map, Action action)
        {
            HeaderText = (map.Beatmap.IsNotNull() && map.Beatmap.Metadata.TitleUnicode.Length > 0) ? $@"Remove beatmap ""{map.Beatmap.Metadata.TitleUnicode}"" from this round?" : @"Remove the beatmap from this round?";
            Icon = FontAwesome.Solid.Trash;
            DangerousAction = action;
        }
    }
}
