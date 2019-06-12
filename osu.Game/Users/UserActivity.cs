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

        public class Modding : UserActivity
        {
            public override string Status => "Modding a map";
            public override Color4 GetAppropriateColour(OsuColour colours) => colours.PurpleDark;
        }

        public class ChoosingBeatmap : UserActivity
        {
            public override string Status => "Choosing a beatmap";
        }

        public class MultiplayerGame : UserActivity
        {
            public override string Status => "Playing with others";
        }

        public class Editing : UserActivity
        {
            public BeatmapInfo Beatmap { get; }

            public Editing(BeatmapInfo info)
            {
                Beatmap = info;
            }

            public override string Status => @"Editing a beatmap";
        }

        public class SoloGame : UserActivity
        {
            public BeatmapInfo Beatmap { get; }

            public Rulesets.RulesetInfo Ruleset { get; }

            public SoloGame(BeatmapInfo info, Rulesets.RulesetInfo ruleset)
            {
                Beatmap = info;
                Ruleset = ruleset;
            }

            public override string Status => @"Playing alone";
        }

        public class Spectating : UserActivity
        {
            public override string Status => @"Spectating a game";
        }

        public class InLobby : UserActivity
        {
            public override string Status => @"In a Multiplayer Lobby";
        }
    }
}
