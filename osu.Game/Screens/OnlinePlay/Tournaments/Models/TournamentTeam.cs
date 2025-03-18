// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Bindables;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Models
{
    /// <summary>
    /// A team representation. For official tournaments this is generally a country.
    /// </summary>
    [Serializable]
    public class TournamentTeam
    {
        /// <summary>
        /// Unique ID for this team. If a team is removed and then added, the ID will not be reused.
        /// </summary>
        public int ID;

        /// <summary>
        /// The name of this team.
        /// </summary>
        public Bindable<string> FullName = new Bindable<string>(string.Empty);

        // todo : Add option to add team image.

        /// <summary>
        /// Name of the file containing the flag.
        /// </summary>
        public Bindable<string> FlagName = new Bindable<string>(string.Empty);

        /// <summary>
        /// Short acronym which appears in the group boxes post-selection.
        /// </summary>
        public Bindable<string> Acronym = new Bindable<string>(string.Empty);

        // Will be also be stored seperately
        public BindableList<SeedingResult> SeedingResults = new BindableList<SeedingResult>();

        /// <summary>
        /// Chronologically sorted list of TournamentMatch.IDs this team has played in.
        /// Negative values means the match was a loss.
        /// </summary>
        public BindableList<int> PlayedMatches = new BindableList<int>();

        public double AverageRank
        {
            get
            {
                int[] ranks = Players.Select(p => p.Rank)
                                     .Where(i => i.HasValue)
                                     .Select(i => i!.Value)
                                     .ToArray();

                if (ranks.Length == 0)
                    return 0;

                return ranks.Average();
            }
        }

        public Bindable<string> Seed = new Bindable<string>(string.Empty);

        public Bindable<int> LastYearPlacing = new BindableInt
        {
            MinValue = 0,
            MaxValue = 256
        };

        [JsonProperty]
        public BindableList<TournamentUser> Players { get; } = new BindableList<TournamentUser>();

        public TournamentTeam()
        {
            Acronym.ValueChanged += val =>
            {
                // use a sane default flag name based on acronym.
                if (val.OldValue.StartsWith(FlagName.Value, StringComparison.InvariantCultureIgnoreCase))
                    FlagName.Value = val.NewValue?.Length >= 2 ? val.NewValue.Substring(0, 2).ToUpperInvariant() : string.Empty;
            };

            FullName.ValueChanged += val =>
            {
                // use a sane acronym based on full name.
                if (val.OldValue.StartsWith(Acronym.Value, StringComparison.InvariantCultureIgnoreCase))
                    Acronym.Value = val.NewValue?.Length >= 3 ? val.NewValue.Substring(0, 3).ToUpperInvariant() : string.Empty;
            };
        }

        public TournamentTeam(ICollection<TournamentUser> players)
        {
            Acronym.ValueChanged += val =>
            {
                // use a sane default flag name based on acronym.
                if (val.OldValue.StartsWith(FlagName.Value, StringComparison.InvariantCultureIgnoreCase))
                    FlagName.Value = val.NewValue?.Length >= 2 ? val.NewValue.Substring(0, 2).ToUpperInvariant() : string.Empty;
            };

            FullName.ValueChanged += val =>
            {
                // use a sane acronym based on full name.
                if (val.OldValue.StartsWith(Acronym.Value, StringComparison.InvariantCultureIgnoreCase))
                    Acronym.Value = val.NewValue?.Length >= 3 ? val.NewValue.Substring(0, 3).ToUpperInvariant() : string.Empty;
            };

            Players.AddRange(players);
        }

        public override string ToString() => FullName.Value ?? Acronym.Value;
    }
}
