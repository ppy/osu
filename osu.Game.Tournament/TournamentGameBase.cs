// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Drawing;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using osu.Game.Tournament.IPC;
using osu.Game.Tournament.Models;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tournament
{
    public abstract class TournamentGameBase : OsuGameBase
    {
        private const string bracket_filename = "bracket.json";

        private LadderInfo ladder;

        private Storage storage;

        private DependencyContainer dependencies;

        private Bindable<Size> windowSize;
        private FileBasedIPC ipc;

        private Drawable heightWarning;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            return dependencies = new DependencyContainer(base.CreateChildDependencies(parent));
        }

        [BackgroundDependencyLoader]
        private void load(Storage storage, FrameworkConfigManager frameworkConfig)
        {
            Resources.AddStore(new DllResourceStore(@"osu.Game.Tournament.dll"));

            Fonts.AddStore(new GlyphStore(Resources, @"Resources/Fonts/Aquatico-Regular"));
            Fonts.AddStore(new GlyphStore(Resources, @"Resources/Fonts/Aquatico-Light"));

            Textures.AddStore(new TextureLoaderStore(new ResourceStore<byte[]>(new StorageBackedResourceStore(storage))));

            this.storage = storage;

            windowSize = frameworkConfig.GetBindable<Size>(FrameworkSetting.WindowedSize);
            windowSize.BindValueChanged(size => ScheduleAfterChildren(() =>
            {
                var minWidth = (int)(size.NewValue.Height / 9f * 16 + 400);

                heightWarning.Alpha = size.NewValue.Width < minWidth ? 1 : 0;
            }), true);

            readBracket();

            ladder.CurrentMatch.Value = ladder.Matches.FirstOrDefault(p => p.Current.Value);

            dependencies.CacheAs<MatchIPCInfo>(ipc = new FileBasedIPC());
            Add(ipc);

            AddRange(new[]
            {
                new OsuButton
                {
                    Text = "Save Changes",
                    Width = 140,
                    Height = 50,
                    Depth = float.MinValue,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Padding = new MarginPadding(10),
                    Action = SaveChanges,
                },
                heightWarning = new Container
                {
                    Masking = true,
                    CornerRadius = 5,
                    Depth = float.MinValue,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.Red,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new SpriteText
                        {
                            Text = "Please make the window wider",
                            Font = OsuFont.Default.With(weight: "bold"),
                            Colour = Color4.White,
                            Padding = new MarginPadding(20)
                        }
                    }
                },
            });
        }

        private void readBracket()
        {
            if (storage.Exists(bracket_filename))
            {
                using (Stream stream = storage.GetStream(bracket_filename, FileAccess.Read, FileMode.Open))
                using (var sr = new StreamReader(stream))
                    ladder = JsonConvert.DeserializeObject<LadderInfo>(sr.ReadToEnd());
            }
            else
            {
                ladder = new LadderInfo();
            }

            dependencies.Cache(ladder);

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
            foreach (var id in round.Matches)
            {
                var found = ladder.Matches.FirstOrDefault(p => p.ID == id);

                if (found != null)
                {
                    found.Round.Value = round;
                    if (round.StartDate.Value > found.Date.Value)
                        found.Date.Value = round.StartDate.Value;
                }
            }

            addedInfo |= addPlayers();
            addedInfo |= addBeatmaps();

            if (addedInfo)
                SaveChanges();
        }

        /// <summary>
        /// Add missing player info based on user IDs.
        /// </summary>
        /// <returns></returns>
        private bool addPlayers()
        {
            bool addedInfo = false;

            foreach (var t in ladder.Teams)
            foreach (var p in t.Players)
                if (string.IsNullOrEmpty(p.Username))
                {
                    var req = new GetUserRequest(p.Id);
                    req.Perform(API);
                    p.Username = req.Result.Username;

                    addedInfo = true;
                }

            return addedInfo;
        }

        /// <summary>
        /// Add missing beatmap info based on beatmap IDs
        /// </summary>
        private bool addBeatmaps()
        {
            bool addedInfo = false;

            foreach (var r in ladder.Rounds)
            foreach (var b in r.Beatmaps)
                if (b.BeatmapInfo == null)
                {
                    var req = new GetBeatmapRequest(new BeatmapInfo { OnlineBeatmapID = b.ID });
                    req.Perform(API);
                    b.BeatmapInfo = req.Result?.ToBeatmap(RulesetStore);

                    addedInfo = true;
                }

            return addedInfo;
        }

        protected override void LoadComplete()
        {
            MenuCursorContainer.Cursor.AlwaysPresent = true; // required for tooltip display
            MenuCursorContainer.Cursor.Alpha = 0;

            base.LoadComplete();
        }

        protected virtual void SaveChanges()
        {
            foreach (var r in ladder.Rounds)
                r.Matches = ladder.Matches.Where(p => p.Round.Value == r).Select(p => p.ID).ToList();

            ladder.Progressions = ladder.Matches.Where(p => p.Progression.Value != null).Select(p => new TournamentProgression(p.ID, p.Progression.Value.ID)).Concat(
                                            ladder.Matches.Where(p => p.LosersProgression.Value != null).Select(p => new TournamentProgression(p.ID, p.LosersProgression.Value.ID, true)))
                                        .ToList();

            using (var stream = storage.GetStream(bracket_filename, FileAccess.Write, FileMode.Create))
            using (var sw = new StreamWriter(stream))
            {
                sw.Write(JsonConvert.SerializeObject(ladder,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                    }));
            }
        }

        protected override UserInputManager CreateUserInputManager() => new TournamentInputManager();

        private class TournamentInputManager : UserInputManager
        {
            protected override MouseButtonEventManager CreateButtonManagerFor(MouseButton button)
            {
                switch (button)
                {
                    case MouseButton.Right:
                        return new RightMouseManager(button);
                }

                return base.CreateButtonManagerFor(button);
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
