// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.OsuDifficulty;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Osu.Scoring;
using osu.Game.Rulesets.Osu.Edit;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Rulesets.Osu
{
    public class OsuRuleset : Ruleset
    {
        public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap, bool isForCurrentRuleset) => new OsuRulesetContainer(this, beatmap, isForCurrentRuleset);

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.Z, OsuAction.LeftButton),
            new KeyBinding(InputKey.X, OsuAction.RightButton),
            new KeyBinding(InputKey.MouseLeft, OsuAction.LeftButton),
            new KeyBinding(InputKey.MouseRight, OsuAction.RightButton),
        };

        public override IEnumerable<BeatmapStatistic> GetBeatmapStatistics(WorkingBeatmap beatmap)
        {
            IEnumerable<HitObject> hitObjects = beatmap.Beatmap.HitObjects;
            IEnumerable<HitObject> circles = hitObjects.Where(c => !(c is IHasEndTime));
            IEnumerable<HitObject> sliders = hitObjects.Where(s => s is IHasCurve);
            IEnumerable<HitObject> spinners = hitObjects.Where(s => s is IHasEndTime && !(s is IHasCurve));

            return new[]
            {
                new BeatmapStatistic
                {
                    Name = @"Circle Count",
                    Content = circles.Count().ToString(),
                    Icon = FontAwesome.fa_circle_o
                },
                new BeatmapStatistic
                {
                    Name = @"Slider Count",
                    Content = sliders.Count().ToString(),
                    Icon = FontAwesome.fa_circle
                },
                new BeatmapStatistic
                {
                    Name = @"Spinner Count",
                    Content = spinners.Count().ToString(),
                    Icon = FontAwesome.fa_circle
                }
            };
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new OsuModEasy(),
                        new OsuModNoFail(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new OsuModHalfTime(),
                                new OsuModDaycore(),
                            },
                        },
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new OsuModHardRock(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new OsuModSuddenDeath(),
                                new OsuModPerfect(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new OsuModDoubleTime(),
                                new OsuModNightcore(),
                            },
                        },
                        new OsuModHidden(),
                        new OsuModFlashlight(),
                    };

                case ModType.Special:
                    return new Mod[]
                    {
                        new OsuModRelax(),
                        new OsuModAutopilot(),
                        new OsuModSpunOut(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new OsuModAutoplay(),
                                new ModCinema(),
                            },
                        },
                        new OsuModTarget(),
                    };

                default:
                    return new Mod[] { };
            }
        }

        public override Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.fa_osu_osu_o };

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap, Mod[] mods = null) => new OsuDifficultyCalculator(beatmap, mods);

        public override PerformanceCalculator CreatePerformanceCalculator(Beatmap beatmap, Score score) => new OsuPerformanceCalculator(this, beatmap, score);

        public override HitObjectComposer CreateHitObjectComposer() => new OsuHitObjectComposer(this);

        public override string Description => "osu!";

        public override string ShortName => "osu";

        public override SettingsSubsection CreateSettings() => new OsuSettings();

        public override int? LegacyID => 0;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new OsuReplayFrame();

        public OsuRuleset(RulesetInfo rulesetInfo = null)
            : base(rulesetInfo)
        {
        }
    }
}
