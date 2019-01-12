// Copyright (c) 2007-2019 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.IO.Enumeration;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class SliderTestCaseInput : SliderTestBase
    {
        private const double testInterval = 500;
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected override List<Mod> Mods { get; set; }

        public SliderTestCaseInput()
        {
            Mods = new List<Mod>();
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Test Slider Tracking", () => trackingInputTest());
        }

        private void trackingInputTest()
        {
            List<List<OsuAction>> actions = new List<List<OsuAction>>();

            List<OsuAction> frame1 = new List<OsuAction>();
            frame1.Add(OsuAction.LeftButton);
            frame1.Add(OsuAction.RightButton);
            actions.Add(frame1);

            List<OsuAction> frame2 = new List<OsuAction>();
            frame2.Add(OsuAction.LeftButton);
            actions.Add(frame2);

            performInputTest(actions, OsuAction.LeftButton);
        }

        private void performInputTest(List<List<OsuAction>> actions, OsuAction? preAction = null)
        {
            List<ReplayFrame> frames = new List<ReplayFrame>();

            Slider sliderToAdd = CreateSlider(3f, 400f, 0, 0.5f, addToContent: false);

            Ruleset ruleset = new OsuRuleset();

            sliderToAdd.Position = ToScreenSpace(sliderToAdd.Position);

            Beatmap<OsuHitObject> beatmap = createSpontaneous(sliderToAdd, ruleset);

            Replay thisReplay = generateReplay(beatmap, actions, preAction);

            Child = loadPlayerFor(ruleset, sliderToAdd, thisReplay, beatmap);
        }

        protected Player CreatePlayer(Ruleset ruleset) => new Player
        {
            AllowPause = false,
            AllowLeadIn = false,
            AllowResults = false,
        };

        private Replay generateReplay(Beatmap<OsuHitObject> b, List<List<OsuAction>> actions, OsuAction? preAction = null)
        {
            Replay replay = new Replay();
            List<ReplayFrame> frames = new List<ReplayFrame>();
            int localIndex = 0;

            Slider s = b.HitObjects[0] as Slider;

            if (preAction.HasValue)
            {
                OsuReplayFrame f = new OsuReplayFrame();
                Vector2 pos = new Vector2(0, 0);
                f.Position = pos;
                f.Time = s.StartTime - 100;
                f.Actions.Add(preAction.Value);

                frames.Insert(localIndex, f);
                localIndex++;
            }

            for (double j = 20; j < s.Duration; j += 20)
            {
                OsuReplayFrame f = new OsuReplayFrame();
                Vector2 pos = s.StackedPositionAt(j / s.Duration);
                f.Position = pos;
                f.Time = s.StartTime + (localIndex * 20);

                frames.Insert(localIndex, f);
                localIndex++;
            }

            replay.Frames = frames;
            return replay;
        }

        private Beatmap<OsuHitObject> createSpontaneous(Slider s, Ruleset r)
        {
            Beatmap<OsuHitObject> b = new Beatmap<OsuHitObject>();
            b.HitObjects.Add(s);
            b.BeatmapInfo.Ruleset = r.RulesetInfo;
            b.ControlPointInfo.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = 0.5f });
            b.BeatmapInfo.BaseDifficulty.SliderTickRate = 3;
            return b;
        }

        private Player loadPlayerFor(Ruleset r, Slider s, Replay rp, Beatmap<OsuHitObject> b)
        {
            var beatmap = b;
            var working = new TestWorkingBeatmap(beatmap);

            Beatmap.Value = working;
            Beatmap.Value.Mods.Value = Beatmap.Value.Mods.Value.Concat(new[] { new ModTestplay(rp) });

            var player = CreatePlayer(r);

            return player;
        }

        private class ModTestplay : OsuModAutoplay
        {
            private Replay replayLocal;

            public ModTestplay(Replay replay)
            {
                replayLocal = replay;
            }

            protected override Score CreateReplayScore(Beatmap<OsuHitObject> beatmap) => new Score
            {
                ScoreInfo = new ScoreInfo { User = new User { Username = "Wangs" } },
                Replay = replayLocal
            };
        }
    }
}
