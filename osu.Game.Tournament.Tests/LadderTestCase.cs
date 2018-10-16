// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
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
        protected LadderInfo Ladder;

        [Resolved]
        private APIAccess api { get; set; } = null;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null;

        [BackgroundDependencyLoader]
        private void load()
        {
            Ladder = File.Exists(@"bracket.json") ? JsonConvert.DeserializeObject<LadderInfo>(File.ReadAllText(@"bracket.json")) : new LadderInfo();

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
            File.WriteAllText(@"bracket.json", JsonConvert.SerializeObject(Ladder,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }));
        }
    }
}
