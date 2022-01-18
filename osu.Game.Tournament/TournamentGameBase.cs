// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.IO.Stores;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Tournament.IO;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK.Input;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tournament
{
    [Cached(typeof(TournamentGameBase))]
    public class TournamentGameBase : OsuGameBase
    {
        public const string BRACKET_FILENAME = @"bracket.json";
        private LadderInfo ladder;
        private TournamentStorage storage;
        private DependencyContainer dependencies;
        private FileBasedIPC ipc;

        protected Task BracketLoadTask => bracketLoadTaskCompletionSource.Task;

        private readonly TaskCompletionSource<bool> bracketLoadTaskCompletionSource = new TaskCompletionSource<bool>();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
        }

        private TournamentSpriteText initialisationText;

        [BackgroundDependencyLoader]
        private void load(Storage baseStorage)
        {
            AddInternal(initialisationText = new TournamentSpriteText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Font = OsuFont.Torus.With(size: 32),
            });

            Resources.AddStore(new DllResourceStore(typeof(TournamentGameBase).Assembly));

            dependencies.CacheAs<Storage>(storage = new TournamentStorage(baseStorage));
            dependencies.CacheAs(storage);

            dependencies.Cache(new TournamentVideoResourceStore(storage));

            Textures.AddStore(new TextureLoaderStore(new StorageBackedResourceStore(storage)));

            dependencies.CacheAs(new StableInfo(storage));

            Task.Run(readBracket);
        }

        private void readBracket()
        {
            try
            {
                if (storage.Exists(BRACKET_FILENAME))
                {
                    using (Stream stream = storage.GetStream(BRACKET_FILENAME, FileAccess.Read, FileMode.Open))
                    using (var sr = new StreamReader(stream))
                        ladder = JsonConvert.DeserializeObject<LadderInfo>(sr.ReadToEnd(), new JsonPointConverter());
                }

                ladder ??= new LadderInfo();

                ladder.Ruleset.Value = ladder.Ruleset.Value != null
                    ? RulesetStore.GetRuleset(ladder.Ruleset.Value.ShortName)
                    : RulesetStore.AvailableRulesets.First();

                bool addedInfo = false;

                // assign teams
                foreach (var match in ladder.Matches)
                {
                    match.Team1.Value = ladder.Teams.FirstOrDefault(t => t.Acronym.Value == match.Team1Acronym);
                    match.Team2.Value = ladder.Teams.FirstOrDefault(t => t.Acronym.Value == match.Team2Acronym);

                    foreach (var conditional in match.ConditionalMatches)
                    {
                        conditional.Team1.Value = ladder.Teams.FirstOrDefault(t => t.Acronym.Value == conditional.Team1Acronym);
                        conditional.Team2.Value = ladder.Teams.FirstOrDefault(t => t.Acronym.Value == conditional.Team2Acronym);
                        conditional.Round.Value = match.Round.Value;
                    }
                }

                // assign progressions
                foreach (var pair in ladder.Progressions)
                {
                    var src = ladder.Matches.FirstOrDefault(p => p.ID == pair.SourceID);
                    var dest = ladder.Matches.FirstOrDefault(p => p.ID == pair.TargetID);

                    if (src == null)
                        continue;

                    if (dest != null)
                    {
                        if (pair.Losers)
                            src.LosersProgression.Value = dest;
                        else
                            src.Progression.Value = dest;
                    }
                }

                // link matches to rounds
                foreach (var round in ladder.Rounds)
                {
                    foreach (int id in round.Matches)
                    {
                        var found = ladder.Matches.FirstOrDefault(p => p.ID == id);

                        if (found != null)
                        {
                            found.Round.Value = round;
                            if (round.StartDate.Value > found.Date.Value)
                                found.Date.Value = round.StartDate.Value;
                        }
                    }
                }

                addedInfo |= addPlayers();
                addedInfo |= addRoundBeatmaps();
                addedInfo |= addSeedingBeatmaps();

                if (addedInfo)
                    SaveChanges();

                ladder.CurrentMatch.Value = ladder.Matches.FirstOrDefault(p => p.Current.Value);
            }
            catch (Exception e)
            {
                bracketLoadTaskCompletionSource.SetException(e);
                return;
            }

            Schedule(() =>
            {
                Ruleset.BindTo(ladder.Ruleset);

                dependencies.Cache(ladder);
                dependencies.CacheAs<MatchIPCInfo>(ipc = new FileBasedIPC());
                Add(ipc);

                bracketLoadTaskCompletionSource.SetResult(true);

                initialisationText.Expire();
            });
        }

        /// <summary>
        /// Add missing player info based on user IDs.
        /// </summary>
        private bool addPlayers()
        {
            var playersRequiringPopulation = ladder.Teams
                                                   .SelectMany(t => t.Players)
                                                   .Where(p => string.IsNullOrEmpty(p.Username)
                                                               || p.Statistics?.GlobalRank == null
                                                               || p.Statistics?.CountryRank == null).ToList();

            if (playersRequiringPopulation.Count == 0)
                return false;

            for (int i = 0; i < playersRequiringPopulation.Count; i++)
            {
                var p = playersRequiringPopulation[i];
                PopulateUser(p, immediate: true);
                updateLoadProgressMessage($"Populating user stats ({i} / {playersRequiringPopulation.Count})");
            }

            return true;
        }

        /// <summary>
        /// Add missing beatmap info based on beatmap IDs
        /// </summary>
        private bool addRoundBeatmaps()
        {
            var beatmapsRequiringPopulation = ladder.Rounds
                                                    .SelectMany(r => r.Beatmaps)
                                                    .Where(b => string.IsNullOrEmpty(b.Beatmap?.BeatmapSet?.Title) && b.ID > 0).ToList();

            if (beatmapsRequiringPopulation.Count == 0)
                return false;

            for (int i = 0; i < beatmapsRequiringPopulation.Count; i++)
            {
                var b = beatmapsRequiringPopulation[i];

                var req = new GetBeatmapRequest(new APIBeatmap { OnlineID = b.ID });
                API.Perform(req);
                b.Beatmap = req.Response ?? new APIBeatmap();

                updateLoadProgressMessage($"Populating round beatmaps ({i} / {beatmapsRequiringPopulation.Count})");
            }

            return true;
        }

        /// <summary>
        /// Add missing beatmap info based on beatmap IDs
        /// </summary>
        private bool addSeedingBeatmaps()
        {
            var beatmapsRequiringPopulation = ladder.Teams
                                                    .SelectMany(r => r.SeedingResults)
                                                    .SelectMany(r => r.Beatmaps)
                                                    .Where(b => string.IsNullOrEmpty(b.Beatmap?.BeatmapSet?.Title) && b.ID > 0).ToList();

            if (beatmapsRequiringPopulation.Count == 0)
                return false;

            for (int i = 0; i < beatmapsRequiringPopulation.Count; i++)
            {
                var b = beatmapsRequiringPopulation[i];

                var req = new GetBeatmapRequest(new APIBeatmap { OnlineID = b.ID });
                API.Perform(req);
                b.Beatmap = req.Response ?? new APIBeatmap();

                updateLoadProgressMessage($"Populating seeding beatmaps ({i} / {beatmapsRequiringPopulation.Count})");
            }

            return true;
        }

        private void updateLoadProgressMessage(string s) => Schedule(() => initialisationText.Text = s);

        public void PopulateUser(APIUser user, Action success = null, Action failure = null, bool immediate = false)
        {
            var req = new GetUserRequest(user.Id, Ruleset.Value);

            if (immediate)
            {
                API.Perform(req);
                populate();
            }
            else
            {
                req.Success += res => { populate(); };
                req.Failure += _ =>
                {
                    user.Id = 1;
                    failure?.Invoke();
                };

                API.Queue(req);
            }

            void populate()
            {
                var res = req.Response;

                if (res == null)
                    return;

                user.Id = res.Id;

                user.Username = res.Username;
                user.Statistics = res.Statistics;
                user.Country = res.Country;
                user.Cover = res.Cover;

                success?.Invoke();
            }
        }

        protected override void LoadComplete()
        {
            MenuCursorContainer.Cursor.AlwaysPresent = true; // required for tooltip display

            // we don't want to show the menu cursor as it would appear on stream output.
            MenuCursorContainer.Cursor.Alpha = 0;

            base.LoadComplete();
        }

        protected virtual void SaveChanges()
        {
            if (!bracketLoadTaskCompletionSource.Task.IsCompletedSuccessfully)
            {
                Logger.Log("Inhibiting bracket save as bracket parsing failed");
                return;
            }

            foreach (var r in ladder.Rounds)
                r.Matches = ladder.Matches.Where(p => p.Round.Value == r).Select(p => p.ID).ToList();

            ladder.Progressions = ladder.Matches.Where(p => p.Progression.Value != null).Select(p => new TournamentProgression(p.ID, p.Progression.Value.ID)).Concat(
                                            ladder.Matches.Where(p => p.LosersProgression.Value != null).Select(p => new TournamentProgression(p.ID, p.LosersProgression.Value.ID, true)))
                                        .ToList();

            // Serialise before opening stream for writing, so if there's a failure it will leave the file in the previous state.
            string serialisedLadder = JsonConvert.SerializeObject(ladder,
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore,
                    Converters = new JsonConverter[] { new JsonPointConverter() }
                });

            using (var stream = storage.GetStream(BRACKET_FILENAME, FileAccess.Write, FileMode.Create))
            using (var sw = new StreamWriter(stream))
                sw.Write(serialisedLadder);
        }

        protected override UserInputManager CreateUserInputManager() => new TournamentInputManager();

        private class TournamentInputManager : UserInputManager
        {
            protected override MouseButtonEventManager CreateButtonEventManagerFor(MouseButton button)
            {
                switch (button)
                {
                    case MouseButton.Right:
                        return new RightMouseManager(button);
                }

                return base.CreateButtonEventManagerFor(button);
            }

            private class RightMouseManager : MouseButtonEventManager
            {
                public RightMouseManager(MouseButton button)
                    : base(button)
                {
                }

                public override bool EnableDrag => true; // allow right-mouse dragging for absolute scroll in scroll containers.
                public override bool EnableClick => true;
                public override bool ChangeFocusOnClick => false;
            }
        }
    }
}
