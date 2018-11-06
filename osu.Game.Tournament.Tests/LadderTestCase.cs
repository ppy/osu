// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Tests.Visual;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests
{
    public abstract class LadderTestCase : OsuTestCase
    {
        private const string bracket_filename = "bracket.json";

        protected LadderInfo Ladder;
        private Storage storage;

        [Resolved]
        private APIAccess api { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

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

            bool addedInfo = false;

            foreach (var g in Ladder.Groupings)
            foreach (var b in g.Beatmaps)
            {
                if (b.BeatmapInfo == null)
                {
                    var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = b.ID });
                    req.Success += i => b.BeatmapInfo = i.ToBeatmap(rulesets);
                    req.Perform(api);

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
