// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Rulesets;

namespace osu.Game.Tournament.Models
{
    /// <summary>
    /// Holds the complete data required to operate the tournament system.
    /// </summary>
    [Serializable]
    public class LadderInfo
    {
        public Bindable<RulesetInfo?> Ruleset = new Bindable<RulesetInfo?>();

        public BindableList<TournamentMatch> Matches = new BindableList<TournamentMatch>();
        public BindableList<TournamentRound> Rounds = new BindableList<TournamentRound>();
        public BindableList<TournamentTeam> Teams = new BindableList<TournamentTeam>();

        // only used for serialisation
        public List<TournamentProgression> Progressions = new List<TournamentProgression>();

        [JsonIgnore] // updated manually in TournamentGameBase
        public Bindable<TournamentMatch?> CurrentMatch = new Bindable<TournamentMatch?>();

        public Bindable<int> ChromaKeyWidth = new BindableInt(1024)
        {
            MinValue = 640,
            MaxValue = 1366,
        };

        public Bindable<int> PlayersPerTeam = new BindableInt(4)
        {
            MinValue = 3,
            MaxValue = 4,
        };

        public Bindable<bool> UseRefereeCommands = new BindableBool(false);

        public Bindable<bool> NeedRefereeResponse = new BindableBool(false);

        public Bindable<bool> AutoProgressScreens = new BindableBool(true);

        public Bindable<bool> SplitMapPoolByMods = new BindableBool(true);

        public Bindable<bool> DisplayTeamSeeds = new BindableBool(false);

        public BindableList<KeyValuePair<BackgroundVideo, string>> BackgroundVideoFiles = new BindableList<KeyValuePair<BackgroundVideo, string>>()
        {
            KeyValuePair.Create(BackgroundVideo.Gameplay, "gameplay"),
            KeyValuePair.Create(BackgroundVideo.Mappool, "mappool"),
            KeyValuePair.Create(BackgroundVideo.Main, "main"),
            KeyValuePair.Create(BackgroundVideo.Ladder, "ladder"),
            KeyValuePair.Create(BackgroundVideo.Schedule, "schedule"),
            KeyValuePair.Create(BackgroundVideo.Drawings, "drawings"),
            KeyValuePair.Create(BackgroundVideo.Showcase, "showcase"),
            KeyValuePair.Create(BackgroundVideo.Seeding, "seeding"),
            KeyValuePair.Create(BackgroundVideo.TeamIntro, "teamintro"),
            KeyValuePair.Create(BackgroundVideo.RedWin, "teamwin-red"),
            KeyValuePair.Create(BackgroundVideo.BlueWin, "teamwin-blue"),
            KeyValuePair.Create(BackgroundVideo.Draw, "mappool"),
            KeyValuePair.Create(BackgroundVideo.Board, "mappool"),
            KeyValuePair.Create(BackgroundVideo.EXStage, "mappool"),
        };
    }
}
