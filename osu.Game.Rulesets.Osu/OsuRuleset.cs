// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.OsuDifficulty;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Framework.Input.Bindings;

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

        public override IEnumerable<BeatmapStatistic> GetBeatmapStatistics(WorkingBeatmap beatmap) => new[]
            {
            new BeatmapStatistic
            {
                Name = @"Circle count",
                Content = beatmap.Beatmap.HitObjects.Count(h => h is HitCircle).ToString(),
                Icon = FontAwesome.fa_dot_circle_o
            },
            new BeatmapStatistic
            {
                Name = @"Slider count",
                Content = beatmap.Beatmap.HitObjects.Count(h => h is Slider).ToString(),
                Icon = FontAwesome.fa_circle_o
            }
        };

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

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new OsuDifficultyCalculator(beatmap);

        public override string Description => "osu!";

        public override SettingsSubsection CreateSettings() => new OsuSettings();

        public override int LegacyID => 0;

        public OsuRuleset(RulesetInfo rulesetInfo)
            : base(rulesetInfo)
        {
        }
    }
}
