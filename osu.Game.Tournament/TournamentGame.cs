// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Tournament.Screens;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament
{
    public class TournamentGame : OsuGameBase
    {
        private const string bracket_filename = "bracket.json";

        protected LadderInfo Ladder;
        private Storage storage;

        private DependencyContainer dependencies;

        [Cached]
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) => dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(Storage storage)
        {
            this.storage = storage;

            string content = null;
            if (storage.Exists(bracket_filename))
            {
                using (Stream stream = storage.GetStream(bracket_filename, FileAccess.Read, FileMode.Open))
                using (var sr = new StreamReader(stream))
                    content = sr.ReadToEnd();
            }

            Ladder = content != null ? JsonConvert.DeserializeObject<LadderInfo>(content) : new LadderInfo();
            dependencies.Cache(Ladder);

            bool addedInfo = false;

            foreach (var g in Ladder.Groupings)
            foreach (var b in g.Beatmaps)
            {
                if (b.BeatmapInfo == null)
                {
                    var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = b.ID });
                    req.Success += i => b.BeatmapInfo = i.ToBeatmap(RulesetStore);
                    req.Perform(API);

                    addedInfo = true;
                }
            }

            if (addedInfo)
                SaveChanges();

            Add(new OsuButton
            {
                Text = "Save Changes",
                Width = 140,
                Height = 50,
                Anchor = Anchor.BottomRight,
                Origin = Anchor.BottomRight,
                Padding = new MarginPadding(10),
                Action = SaveChanges,
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Add(new TournamentSceneManager());

            MenuCursorContainer.Cursor.Alpha = 0;
        }

        protected virtual void SaveChanges()
        {
            using (var stream = storage.GetStream(bracket_filename, FileAccess.Write, FileMode.Create))
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(JsonConvert.SerializeObject(Ladder,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    }));
            }
        }
    }
}
