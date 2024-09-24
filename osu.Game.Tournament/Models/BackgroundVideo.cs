// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Screens.OnlinePlay.Components;

namespace osu.Game.Tournament.Models
{
    public enum BackgroundVideo
    {
        Main,
        Ladder,
        Schedule,
        Drawings,
        Showcase,
        Seeding,
        TeamIntro,
        Gameplay,
        Mappool,
        RedWin,
        BlueWin,
        // OFFC specific
        Draw,
        Board,
        EXStage,
    }

    public static class BackgroundVideoProps
    {
        public static readonly List<KeyValuePair<BackgroundVideo, string>> DISPLAY_NAMES = new List<KeyValuePair<BackgroundVideo, string>>()
        {
            KeyValuePair.Create(BackgroundVideo.Main, "Main Video"),
            KeyValuePair.Create(BackgroundVideo.Ladder, "Ladder Video"),
            KeyValuePair.Create(BackgroundVideo.Schedule, "Schedule Video"),
            KeyValuePair.Create(BackgroundVideo.Drawings, "Drawings Video"),
            KeyValuePair.Create(BackgroundVideo.Showcase, "Showcase Video"),
            KeyValuePair.Create(BackgroundVideo.Seeding, "Seeding Video"),
            KeyValuePair.Create(BackgroundVideo.TeamIntro, "Team Intro Video"),
            KeyValuePair.Create(BackgroundVideo.Gameplay, "Gameplay Video"),
            KeyValuePair.Create(BackgroundVideo.Mappool, "Mappool Video"),
            KeyValuePair.Create(BackgroundVideo.RedWin, "Red Win Video"),
            KeyValuePair.Create(BackgroundVideo.BlueWin, "Blue Win Video"),
            // OFFC specific
            KeyValuePair.Create(BackgroundVideo.Draw, "Draw Video"),
            KeyValuePair.Create(BackgroundVideo.Board, "Board Video"),
            KeyValuePair.Create(BackgroundVideo.EXStage, "EX Stage Video"),
        };

        public static string GetDisplayName(BackgroundVideo video) => DISPLAY_NAMES.Find(kvp => kvp.Key == video).Value;

        public static BackgroundVideo GetVideoFromName(string dName) => DISPLAY_NAMES.Find(kvp => kvp.Value == dName).Key;
    }
}
