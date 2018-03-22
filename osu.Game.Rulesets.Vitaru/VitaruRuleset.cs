using System.Collections.Generic;
using osu.Game.Rulesets.UI;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Vitaru.Mods;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Overlays.Settings;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics;
using System.Linq;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Vitaru.Scoring;
using osu.Game.Rulesets.Vitaru.Settings;
using osu.Game.Rulesets.Vitaru.Edit;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Vitaru.Objects;
using osu.Game.Rulesets.Vitaru.UI;
using System;
using osu.Game.Rulesets.Vitaru.Beatmaps;

namespace osu.Game.Rulesets.Vitaru
{
    public class VitaruRuleset : Ruleset
    {
        /// <summary>
        /// Used for Online Multiplayer compatibility
        /// </summary>
        public const string RulesetVersion = "0.8.0";

        public override int? LegacyID => 4; 

        public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap, bool isForCurrentRuleset) => new VitaruRulesetContainer(this, beatmap, isForCurrentRuleset);

        public override string Description => "vitaru!";

        public override string ShortName => "vitaru";

        public override IEnumerable<int> AvailableVariants
        {
            get
            {
                for (int i = 0; i <= 6; i++)
                    yield return (int)ControlScheme.Vitaru + i;
            }
        }

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
        {
            switch (getControlType(variant))
            {
                case ControlScheme.Vitaru:
                    return new KeyBinding[]
                    {
                        new KeyBinding(InputKey.W, VitaruAction.Up),
                        new KeyBinding(InputKey.S, VitaruAction.Down),
                        new KeyBinding(InputKey.A, VitaruAction.Left),
                        new KeyBinding(InputKey.D, VitaruAction.Right),
                        new KeyBinding(InputKey.MouseLeft, VitaruAction.Shoot),
                        new KeyBinding(InputKey.Shift, VitaruAction.Slow),
                        new KeyBinding(InputKey.Space, VitaruAction.Fast),
                    };
                case ControlScheme.Dodge:
                    return new KeyBinding[]
                    {
                        new KeyBinding(InputKey.W, VitaruAction.Up),
                        new KeyBinding(InputKey.S, VitaruAction.Down),
                        new KeyBinding(InputKey.A, VitaruAction.Left),
                        new KeyBinding(InputKey.D, VitaruAction.Right),
                        new KeyBinding(InputKey.Shift, VitaruAction.Slow),
                    };
                case ControlScheme.Touhosu:
                    return new KeyBinding[]
                    {
                        new KeyBinding(InputKey.W, VitaruAction.Up),
                        new KeyBinding(InputKey.S, VitaruAction.Down),
                        new KeyBinding(InputKey.A, VitaruAction.Left),
                        new KeyBinding(InputKey.D, VitaruAction.Right),
                        new KeyBinding(InputKey.MouseLeft, VitaruAction.Shoot),
                        new KeyBinding(InputKey.MouseRight, VitaruAction.Spell),
                        new KeyBinding(InputKey.Shift, VitaruAction.Slow),
                    };
                case ControlScheme.Sakuya:
                    return new KeyBinding[]
                    {
                        new KeyBinding(InputKey.W, VitaruAction.Up),
                        new KeyBinding(InputKey.S, VitaruAction.Down),
                        new KeyBinding(InputKey.A, VitaruAction.Left),
                        new KeyBinding(InputKey.D, VitaruAction.Right),
                        new KeyBinding(InputKey.MouseLeft, VitaruAction.Shoot),
                        new KeyBinding(InputKey.MouseRight, VitaruAction.Spell),
                        new KeyBinding(InputKey.E, VitaruAction.Increase),
                        new KeyBinding(InputKey.Q, VitaruAction.Decrease),
                        new KeyBinding(InputKey.Shift, VitaruAction.Slow),
                    };
                case ControlScheme.Kokoro:
                    return new KeyBinding[]
                    {
                        new KeyBinding(InputKey.W, VitaruAction.Up),
                        new KeyBinding(InputKey.S, VitaruAction.Down),
                        new KeyBinding(InputKey.A, VitaruAction.Left),
                        new KeyBinding(InputKey.D, VitaruAction.Right),
                        new KeyBinding(InputKey.MouseLeft, VitaruAction.Shoot),
                        new KeyBinding(InputKey.MouseRight, VitaruAction.Spell),
                        new KeyBinding(InputKey.E, VitaruAction.RightShoot),
                        new KeyBinding(InputKey.Q, VitaruAction.LeftShoot),
                        new KeyBinding(InputKey.Shift, VitaruAction.Slow),
                    };
                case ControlScheme.NueHoujuu:
                    return new KeyBinding[]
                    {
                        new KeyBinding(InputKey.W, VitaruAction.Up),
                        new KeyBinding(InputKey.S, VitaruAction.Down),
                        new KeyBinding(InputKey.A, VitaruAction.Left),
                        new KeyBinding(InputKey.D, VitaruAction.Right),
                        new KeyBinding(InputKey.MouseLeft, VitaruAction.Shoot),
                        new KeyBinding(InputKey.MouseRight, VitaruAction.Spell),
                        new KeyBinding(InputKey.MouseMiddle, VitaruAction.Spell2),
                        new KeyBinding(InputKey.E, VitaruAction.Spell3),
                        new KeyBinding(InputKey.Q, VitaruAction.Spell4),
                        new KeyBinding(InputKey.Shift, VitaruAction.Slow),
                    };
                case ControlScheme.AliceMuyart:
                    return new KeyBinding[]
                    {
                        new KeyBinding(InputKey.W, VitaruAction.Up),
                        new KeyBinding(InputKey.S, VitaruAction.Down),
                        new KeyBinding(InputKey.A, VitaruAction.Left),
                        new KeyBinding(InputKey.D, VitaruAction.Right),
                        new KeyBinding(InputKey.MouseLeft, VitaruAction.Shoot),
                        new KeyBinding(InputKey.MouseRight, VitaruAction.Spell),
                        new KeyBinding(InputKey.MouseButton1, VitaruAction.Spell2),
                        new KeyBinding(InputKey.MouseButton2, VitaruAction.Spell3),
                        new KeyBinding(InputKey.E, VitaruAction.Increase),
                        new KeyBinding(InputKey.Q, VitaruAction.Decrease),
                        new KeyBinding(InputKey.Shift, VitaruAction.Slow),
                    };
            }

            return new KeyBinding[0];
        }

        public override string GetVariantName(int variant)
        {
            switch (getControlType(variant))
            {
                default:
                    return "null";
                case ControlScheme.Vitaru:
                    return "Vitaru";
                case ControlScheme.Dodge:
                    return "Dodge";
                case ControlScheme.Touhosu:
                    return "Touhosu";
                case ControlScheme.Sakuya:
                    return "Sakuya";
                case ControlScheme.Kokoro:
                    return "Kokoro";
                case ControlScheme.NueHoujuu:
                    return "Nue Houjuu";
                case ControlScheme.AliceMuyart:
                    return "Alice Muyart";
            }
        }

        private ControlScheme getControlType(int variant)
        {
            return (ControlScheme)Enum.GetValues(typeof(ControlScheme)).Cast<int>().OrderByDescending(i => i).First(v => variant >= v);
        }

        public override IEnumerable<BeatmapStatistic> GetBeatmapStatistics(WorkingBeatmap beatmap) => new[]
        {
            new BeatmapStatistic
            {
                Name = @"Pattern count",
                Content = beatmap.Beatmap.HitObjects.Count(h => h is Pattern).ToString(),
                Icon = FontAwesome.fa_dot_circle_o
            }
        };

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap, Mod[] mods = null) => new VitaruDifficultyCalculator(beatmap, mods);

        public override PerformanceCalculator CreatePerformanceCalculator(Beatmap beatmap, Score score) => new VitaruPerformanceCalculator(this, beatmap, score);

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new VitaruModEasy(),
                        new VitaruModNoFail(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new VitaruModHalfTime(),
                                new VitaruModDaycore(),
                            },
                        },
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new VitaruModHardRock(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new VitaruModSuddenDeath(),
                                new VitaruModPerfect(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new VitaruModDoubleTime(),
                                new VitaruModNightcore(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new VitaruModHidden(),
                                new VitaruModFlashlight(),
                            },
                        },
                    };

                case ModType.Special:
                    return new Mod[]
                    {
                        new VitaruRelax(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new VitaruModAutoplay(),
                                new ModCinema(),
                            },
                        },
                    };
                default: return new Mod[] { };
            }
        }

        public override SettingsSubsection CreateSettings() => new VitaruSettings();

        public override Drawable CreateIcon() => new Sprite { Texture = VitaruTextures.Get("icon") };

        public override HitObjectComposer CreateHitObjectComposer() => new VitaruHitObjectComposer(this);

        public static ResourceStore<byte[]> VitaruResources;
        public static TextureStore VitaruTextures;

        public VitaruRuleset(RulesetInfo rulesetInfo) : base(rulesetInfo)
        {
            if (VitaruResources == null)
            {
                VitaruResources = new ResourceStore<byte[]>();
                VitaruResources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore("osu.Game.Rulesets.Vitaru.dll"), ("Assets")));
                VitaruResources.AddStore(new DllResourceStore("osu.Game.Rulesets.Vitaru.dll"));
                VitaruTextures = new TextureStore(new RawTextureLoaderStore(new NamespacedResourceStore<byte[]>(VitaruResources, @"Textures")));
                VitaruTextures.AddStore(new RawTextureLoaderStore(new OnlineStore()));
            }
        }
    }

    public enum VitaruGamemode
    {
        Vitaru,        
        Dodge,
        Touhosu
    }

    public enum ControlScheme
    {
        Vitaru,
        Dodge,

        Touhosu,

        Sakuya,
        Kokoro,
        NueHoujuu,
        AliceMuyart,
    }
}
