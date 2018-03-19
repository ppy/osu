using System.Collections.Generic;
using osu.Game.Rulesets.UI;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Shape.Mods;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Audio;
using osu.Framework.Input.Bindings;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Shape.Settings;

namespace osu.Game.Rulesets.Shape
{
    public class ShapeRuleset : Ruleset
    {
        public override Drawable CreateIcon() => new Sprite { Texture = ShapeTextures.Get("icon") };

        public static ResourceStore<byte[]> ShapeResources;
        public static TextureStore ShapeTextures;
        public static AudioManager ShapeClassicAudio;

        public ShapeRuleset(RulesetInfo rulesetInfo)
            : base(rulesetInfo)
        {
            ShapeResources = new ResourceStore<byte[]>();
            ShapeResources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore("osu.Game.Rulesets.Shape.dll"), ("Assets")));
            ShapeResources.AddStore(new DllResourceStore("osu.Game.Rulesets.Shape.dll"));
            ShapeTextures = new TextureStore(new RawTextureLoaderStore(new NamespacedResourceStore<byte[]>(ShapeResources, @"Textures")));
            ShapeTextures.AddStore(new RawTextureLoaderStore(new OnlineStore()));
        }

        public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap, bool isForCurrentRuleset) => new ShapeRulesetContainer(this, beatmap, isForCurrentRuleset);

        public override int? LegacyID => 5;

        public override string Description => "shape!";

        public override string ShortName => "shape";

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap, Mod[] mods = null) => new ShapeDifficultyCalculator(beatmap, mods);

        public override SettingsSubsection CreateSettings() => new ShapeSettings();

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.A, ShapeAction.EastLeftButton),
            new KeyBinding(InputKey.S, ShapeAction.WestLeftButton),
            new KeyBinding(InputKey.D, ShapeAction.NorthLeftButton),
            new KeyBinding(InputKey.F, ShapeAction.SouthLeftButton),
            new KeyBinding(InputKey.J, ShapeAction.SouthRightButton),
            new KeyBinding(InputKey.K, ShapeAction.NorthRightButton),
            new KeyBinding(InputKey.L, ShapeAction.WestRightButton),
            new KeyBinding(InputKey.Semicolon, ShapeAction.EastRightButton),
        };

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new ShapeModEasy(),
                        new ShapeModNoFail(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ShapeModHalfTime(),
                                new ShapeModDaycore(),
                            },
                        },
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new ShapeModHardRock(),
                        new ShapeModSuddenDeath(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ShapeModDoubleTime(),
                                new ShapeModNightcore(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ShapeModHidden(),
                                new ShapeModFlashlight(),
                            },
                        },
                    };

                case ModType.Special:
                    return new Mod[]
                    {
                        new ShapeRelax()
                    };
                default : return new Mod[] { };
            }
        }
    }
}
