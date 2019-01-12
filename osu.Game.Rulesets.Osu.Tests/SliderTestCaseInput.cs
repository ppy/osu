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
        private readonly Container content;
        protected override Container<Drawable> Content => content;

        protected override List<Mod> Mods { get; set; }

        public SliderTestCaseInput()
        {
            Mods = new List<Mod>();
            base.Content.Add(content = new OsuInputManager(new RulesetInfo { ID = 0 }));

            AddStep("Test key priming", () => correctKeyTest());
        }

        private void correctKeyTest()
        {
            List<List<OsuAction>> actions = new List<List<OsuAction>>();

            List<OsuAction> frame1 = new List<OsuAction>();
            frame1.Add(OsuAction.LeftButton);
            actions.Add(frame1);

            List<OsuAction> frame2 = new List<OsuAction>();
            frame2.Add(OsuAction.LeftButton);
            frame2.Add(OsuAction.RightButton);
            actions.Add(frame2);

            List<OsuAction> frame3 = new List<OsuAction>();
            frame3.Add(OsuAction.LeftButton);
            actions.Add(frame3);

            performTrackedInputTest(actions, OsuAction.LeftButton);
        }

        private void performTrackedInputTest(List<List<OsuAction>> actions, OsuAction? preAction = null)
        {
            List<ReplayFrame> frames = new List<ReplayFrame>();

            Slider sliderToAdd = CreateSlider(addToContent: false);

            Ruleset ruleset = new OsuRuleset();

            sliderToAdd.Position = ToScreenSpace(sliderToAdd.Position);

            Beatmap<OsuHitObject> beatmap = createBeatmap(sliderToAdd, ruleset);

            Replay thisReplay = generateTrackedReplay(beatmap, actions, true);

            Child = loadPlayerFor(ruleset, sliderToAdd, thisReplay, beatmap);
        }

        protected Player CreatePlayer(Ruleset ruleset, Score score) => new ReplayPlayer(score)
        {
            AllowPause = false,
            AllowLeadIn = false,
            AllowResults = false,

        };

        private Replay generateTrackedReplay(Beatmap<OsuHitObject> b, List<List<OsuAction>> actions, bool preAction = false)
        {
            Replay replay = new Replay();
            List<ReplayFrame> frames = new List<ReplayFrame>();
            int localIndex = 0;
            const double testInterval = 250;

            Slider s = b.HitObjects[0] as Slider;

            if (preAction)
            {
                OsuReplayFrame f = new OsuReplayFrame();
                Vector2 pos = new Vector2(0, 0);
                f.Position = pos;
                f.Time = s.StartTime - testInterval;
                if (localIndex < actions.Count)
                    f.Actions = actions[localIndex];

                frames.Insert(localIndex, f);
                localIndex++;
            }

            for (double j = testInterval; j < s.Duration; j += testInterval)
            {
                OsuReplayFrame f = new OsuReplayFrame();
                Vector2 pos = s.StackedPositionAt(j / s.Duration);
                f.Position = pos;
                f.Time = s.StartTime + (localIndex * testInterval);
                if (localIndex < actions.Count)
                    f.Actions = actions[localIndex];

                frames.Insert(localIndex, f);
                localIndex++;
            }

            replay.Frames = frames;
            return replay;
        }

        private Beatmap<OsuHitObject> createBeatmap(Slider s, Ruleset r)
        {
            Beatmap<OsuHitObject> b = new Beatmap<OsuHitObject>();
            b.HitObjects.Add(s);
            b.ControlPointInfo.DifficultyPoints.Add(new DifficultyControlPoint { SpeedMultiplier = 0.5f });
            b.BeatmapInfo.BaseDifficulty.SliderTickRate = 3;
            b.BeatmapInfo.Ruleset = r.RulesetInfo;
            return b;
        }

        private Player loadPlayerFor(Ruleset r, Slider s, Replay rp, Beatmap<OsuHitObject> b)
        {
            var beatmap = b;
            var working = new TestWorkingBeatmap(beatmap);
            var score = new Score{ Replay = rp };

            Beatmap.Value.Mods.Value = Beatmap.Value.Mods.Value.Concat(new[] { new OsuModNoFail() });
            Beatmap.Value = working;

            var player = CreatePlayer(r, score);

            return player;
        }
    }
}
