// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Drawing;
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
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament
{
    public abstract class TournamentGameBase : OsuGameBase
    {
        private const string bracket_filename = "bracket.json";

        protected LadderInfo Ladder;
        private Storage storage;

        private DependencyContainer dependencies;

        [Cached]
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        private Bindable<Size> windowSize;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage, FrameworkConfigManager frameworkConfig)
        {
            this.storage = storage;

            windowSize = frameworkConfig.GetBindable<Size>(FrameworkSetting.WindowedSize);

            string content = null;
            if (storage.Exists(bracket_filename))
                using (Stream stream = storage.GetStream(bracket_filename, FileAccess.Read, FileMode.Open))
                using (var sr = new StreamReader(stream))
                {
                    content = sr.ReadToEnd();
                }

            Ladder = content != null ? JsonConvert.DeserializeObject<LadderInfo>(content) : new LadderInfo();
            dependencies.Cache(Ladder);

            bool addedInfo = false;

            foreach (var g in Ladder.Groupings)
            foreach (var b in g.Beatmaps)
                if (b.BeatmapInfo == null)
                {
                    var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = b.ID });
                    req.Success += i => b.BeatmapInfo = i.ToBeatmap(RulesetStore);
                    req.Perform(API);

                    addedInfo = true;
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
            MenuCursorContainer.Cursor.Alpha = 0;
        }

        protected override void Update()
        {

            base.Update();
            var minWidth = (int)(windowSize.Value.Height / 9f * 16 + 400);
            if (windowSize.Value.Width < minWidth)
            {
                // todo: can be removed after ppy/osu-framework#1975
                windowSize.Value = Host.Window.ClientSize = new Size(minWidth, windowSize.Value.Height);
            }
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
