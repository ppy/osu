// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Users
{
    public abstract class UserActivity
    {
        public abstract string Status { get; }
        public virtual Color4 GetAppropriateColour(OsuColour colours) => colours.GreenDarker;

        public class UserActivityModding : UserActivity
        {
            public override string Status => "Modding a map";
            public override Color4 GetAppropriateColour(OsuColour colours) => colours.PurpleDark;
        }

        public class UserActivityChoosingBeatmap : UserActivity
        {
            public override string Status => "Choosing a beatmap";
        }

        public class UserActivityMultiplayerGame : UserActivity
        {
            public override string Status => "Multiplaying";
        }

        public class UserActivityEditing : UserActivity
        {
            public UserActivityEditing(BeatmapInfo info)
            {
                Beatmap = info;
            }

            public override string Status => @"Editing a beatmap";

            public BeatmapInfo Beatmap { get; }
        }

        public class UserActivitySoloGame : UserActivity
        {
            public UserActivitySoloGame(BeatmapInfo info, Rulesets.RulesetInfo ruleset)
            {
                Beatmap = info;
                Ruleset = ruleset;
            }

            public override string Status => @"Solo Game";

            public BeatmapInfo Beatmap { get; }

            public Rulesets.RulesetInfo Ruleset { get; }
        }

        public class UserActivitySpectating : UserActivity
        {
            public override string Status => @"Spectating a game";
        }

        public class UserActivityInLobby : UserActivity
        {
            public override string Status => @"in Multiplayer Lobby";
        }
    }
}
