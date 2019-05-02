// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Logging;
using osu.Framework.Platform;

namespace osu.Game.Screens.Tournament.Teams
{
    public class StorageBackedTeamList : ITeamList
    {
        private const string teams_filename = "drawings.txt";

        private readonly Storage storage;

        public StorageBackedTeamList(Storage storage)
        {
            this.storage = storage;
        }

        public IEnumerable<DrawingsTeam> Teams
        {
            get
            {
                var teams = new List<DrawingsTeam>();

                try
                {
                    using (Stream stream = storage.GetStream(teams_filename, FileAccess.Read, FileMode.Open))
                    using (var sr = new StreamReader(stream))
                    {
                        while (sr.Peek() != -1)
                        {
                            string line = sr.ReadLine()?.Trim();

                            if (string.IsNullOrEmpty(line))
                                continue;

                            // ReSharper disable once PossibleNullReferenceException
                            string[] split = line.Split(':');

                            if (split.Length < 2)
                            {
                                Logger.Log($"Invalid team definition: {line}. Expected \"flag_name : team_name : team_acronym\".");
                                continue;
                            }

                            string flagName = split[0].Trim();
                            string teamName = split[1].Trim();

                            string acronym = split.Length >= 3 ? split[2].Trim() : teamName;
                            acronym = acronym.Substring(0, Math.Min(3, acronym.Length));

                            teams.Add(new DrawingsTeam
                            {
                                FlagName = flagName,
                                FullName = teamName,
                                Acronym = acronym
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to read teams.");
                }

                return teams;
            }
        }
    }
}
