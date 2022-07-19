// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Replays;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// A mod which creates full replay data, which is to be played back in place of a local user playing the game.
    /// </summary>
    public interface ICreateReplayData
    {
        /// <summary>
        /// Create replay data.
        /// </summary>
        /// <param name="beatmap">The beatmap to create replay data for.</param>
        /// <param name="mods">The mods to take into account when creating the replay data.</param>
        /// <returns>A <see cref="ModReplayData"/> structure, containing the generated replay data.</returns>
        /// <remarks>
        /// For callers that want to receive a directly usable <see cref="Score"/> instance,
        /// the <see cref="ModExtensions.CreateScoreFromReplayData"/> extension method is provided for convenience.
        /// </remarks>
        ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods);
    }

    /// <summary>
    /// Data created by a mod that implements <see cref="ICreateReplayData"/>.
    /// </summary>
    public class ModReplayData
    {
        /// <summary>
        /// The full replay data.
        /// </summary>
        public readonly Replay Replay;

        /// <summary>
        /// Placeholder user data to show in place of the local user when the associated mod is active.
        /// </summary>
        public readonly ModCreatedUser User;

        public ModReplayData(Replay replay, ModCreatedUser user = null)
        {
            Replay = replay;
            User = user ?? new ModCreatedUser();
        }
    }

    /// <summary>
    /// A user which is associated with a replay that was created by a mod (ie. autoplay or cinema).
    /// </summary>
    public class ModCreatedUser : IUser
    {
        public int OnlineID => APIUser.SYSTEM_USER_ID;
        public CountryCode CountryCode => default;
        public bool IsBot => true;

        public string Username { get; set; } = string.Empty;
    }
}
