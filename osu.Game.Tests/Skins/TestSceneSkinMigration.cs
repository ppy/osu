// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Rendering.Dummy;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.IO;
using osu.Game.Models;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Skins
{
    [HeadlessTest]
    public partial class TestSceneSkinMigration : OsuTestScene
    {
        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        [Test]
        public void TestEmptyConfiguration()
        {
            LegacySkin skin = null!;

            AddStep("load skin with empty configuration", () => skin = loadSkin<LegacySkin>());
            AddAssert("skin has no configuration", () => !skin.LayoutInfos.Any());
        }

        [Test]
        public void TestSomeConfiguration()
        {
            LegacySkin skin = null!;

            AddStep("load skin", () =>
            {
                skin = loadSkin<LegacySkin>(new Dictionary<GlobalSkinnableContainers, SkinLayoutInfo>
                {
                    { GlobalSkinnableContainers.MainHUDComponents, createLayout(SkinLayoutInfo.LATEST_VERSION, [nameof(LegacyHealthDisplay)]) },
                });
            });

            AddAssert("skin has correct configuration", () =>
            {
                return skin.LayoutInfos.Single().Value.DrawableInfo["global"].Select(d => d.Type.Name).SequenceEqual([nameof(BigBlackBox), nameof(LegacyHealthDisplay)]) &&
                       skin.LayoutInfos.Single().Value.DrawableInfo.Where(d => d.Key != @"global")
                           .All(d => d.Value.Single().Type.Name == nameof(BigBlackBox));
            });
        }

        #region Version 1

        [Test]
        public void TestMigration_1()
        {
            LegacySkin skin = null!;

            AddStep("load skin", () =>
            {
                skin = loadSkin<LegacySkin>(new Dictionary<GlobalSkinnableContainers, SkinLayoutInfo>
                {
                    {
                        GlobalSkinnableContainers.MainHUDComponents,
                        createLayout(0, [nameof(LegacyDefaultComboCounter)])
                    },
                });
            });

            AddAssert("combo counter removed from global", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo;
                return dict.Single(kvp => kvp.Key == @"global").Value.Single().Type.Name == nameof(BigBlackBox);
            });
            AddAssert("combo counter moved to each ruleset", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo;
                return dict.Where(kvp => kvp.Key != @"global").All(kvp => kvp.Value.Select(d => d.Type.Name).SequenceEqual([nameof(BigBlackBox), nameof(LegacyDefaultComboCounter)]));
            });
        }

        [Test]
        public void TestMigration_1_NoComboCounter()
        {
            LegacySkin skin = null!;

            AddStep("load skin", () =>
            {
                skin = loadSkin<LegacySkin>(new Dictionary<GlobalSkinnableContainers, SkinLayoutInfo>
                {
                    {
                        GlobalSkinnableContainers.MainHUDComponents,
                        createLayout(0)
                    },
                });
            });

            AddAssert("nothing removed from global", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo;
                return dict.Single(kvp => kvp.Key == @"global").Value.Single().Type.Name == nameof(BigBlackBox);
            });
            AddAssert("no combo counter added", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo;
                return dict.Where(kvp => kvp.Key != @"global").All(kvp => kvp.Value.Select(d => d.Type.Name).SequenceEqual([nameof(BigBlackBox)]));
            });
        }

        #endregion

        #region Version 2

        [Test]
        public void TestMigration_2()
        {
            LegacySkin skin = null!;

            AddStep("load skin", () =>
            {
                skin = loadSkin<LegacySkin>(new Dictionary<GlobalSkinnableContainers, SkinLayoutInfo>
                {
                    {
                        GlobalSkinnableContainers.MainHUDComponents,
                        createLayout(1, [nameof(LegacyHealthDisplay)])
                    },
                    {
                        GlobalSkinnableContainers.Playfield,
                        createLayout(1)
                    }
                });
            });

            // HUD
            AddAssert("health display removed from global HUD", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo;
                return dict.Single(kvp => kvp.Key == @"global").Value.Single().Type.Name == nameof(BigBlackBox);
            });
            AddAssert("health display moved to each ruleset except mania", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo.ToArray();
                dict = dict.Where(kvp => kvp.Key != @"global" && kvp.Key != @"mania").ToArray();

                return dict.All(kvp => kvp.Value.Select(d => d.Type.Name).SequenceEqual([nameof(BigBlackBox), nameof(LegacyHealthDisplay)])) &&
                       dict.All(kvp => kvp.Value.Single(d => d.Type.Name == nameof(LegacyHealthDisplay)).Rotation == 0f);
            });
            AddAssert("no health display in mania HUD", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo;
                return dict.Single(kvp => kvp.Key == @"mania").Value.Single().Type.Name == nameof(BigBlackBox);
            });

            // Playfield
            AddAssert("health display in mania moved to playfield", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.Playfield].DrawableInfo;
                return dict.Single(kvp => kvp.Key == @"mania").Value.Select(d => d.Type.Name).SequenceEqual([nameof(BigBlackBox), nameof(LegacyHealthDisplay)]) &&
                       dict.Single(kvp => kvp.Key == @"mania").Value.Single(d => d.Type.Name == nameof(LegacyHealthDisplay)).Rotation == -90f;
            });
            AddAssert("rest is unaffected", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.Playfield].DrawableInfo;
                return dict.Where(kvp => kvp.Key != @"mania").All(kvp => kvp.Value.Single().Type.Name == nameof(BigBlackBox));
            });
        }

        [Test]
        public void TestMigration_2_NonLegacySkin()
        {
            ArgonSkin skin = null!;

            AddStep("load argon skin", () =>
            {
                skin = loadSkin<ArgonSkin>(new Dictionary<GlobalSkinnableContainers, SkinLayoutInfo>
                {
                    {
                        GlobalSkinnableContainers.MainHUDComponents,
                        createLayout(1, [nameof(LegacyHealthDisplay)])
                    },
                    {
                        GlobalSkinnableContainers.Playfield,
                        createLayout(1)
                    }
                });
            });

            // HUD
            AddAssert("health display still in global HUD", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo;

                return dict.Single(kvp => kvp.Key == @"global").Value.Select(d => d.Type.Name).SequenceEqual([nameof(BigBlackBox), nameof(LegacyHealthDisplay)]) &&
                       dict.Single(kvp => kvp.Key == @"global").Value.Single(d => d.Type.Name == nameof(LegacyHealthDisplay)).Rotation == 0f;
            });
            AddAssert("ruleset HUDs unaffected", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo.ToArray();
                return dict.Where(kvp => kvp.Key != @"global").All(kvp => kvp.Value.Single().Type.Name == nameof(BigBlackBox));
            });

            // Playfield
            AddAssert("playfield unaffected", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.Playfield].DrawableInfo.ToArray();
                return dict.All(kvp => kvp.Value.Single().Type.Name == nameof(BigBlackBox));
            });
        }

        [Test]
        public void TestMigration_2_NoHUD()
        {
            LegacySkin skin = null!;

            AddStep("load skin", () =>
            {
                skin = loadSkin<LegacySkin>(new Dictionary<GlobalSkinnableContainers, SkinLayoutInfo>
                {
                    {
                        GlobalSkinnableContainers.Playfield,
                        createLayout(1)
                    },
                });
            });

            // In this case, we must add a health display to the Playfield target,
            // otherwise on mania the user will not see a health display anymore.

            // HUD
            AddAssert("HUD not configured", () => !skin.LayoutInfos.ContainsKey(GlobalSkinnableContainers.MainHUDComponents));

            // Playfield
            AddAssert("health display in mania moved to playfield", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.Playfield].DrawableInfo;
                return dict.Single(kvp => kvp.Key == @"mania").Value.Select(d => d.Type.Name).SequenceEqual([nameof(BigBlackBox), nameof(LegacyHealthDisplay)]) &&
                       dict.Single(kvp => kvp.Key == @"mania").Value.Single(d => d.Type.Name == nameof(LegacyHealthDisplay)).Rotation == -90f;
            });
            AddAssert("rest is unaffected", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.Playfield].DrawableInfo;
                return dict.Where(kvp => kvp.Key != @"mania").All(d => d.Value.Single().Type.Name == nameof(BigBlackBox));
            });
        }

        [Test]
        public void TestMigration_2_NoPlayfield()
        {
            LegacySkin skin = null!;

            AddStep("load skin", () =>
            {
                skin = loadSkin<LegacySkin>(new Dictionary<GlobalSkinnableContainers, SkinLayoutInfo>
                {
                    {
                        GlobalSkinnableContainers.MainHUDComponents,
                        createLayout(1, [nameof(LegacyHealthDisplay)])
                    }
                });
            });

            // HUD
            AddAssert("health display removed from global HUD", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo;
                return dict.Single(kvp => kvp.Key == @"global").Value.Single().Type.Name == nameof(BigBlackBox);
            });
            AddAssert("health display moved to each ruleset except mania", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo.ToArray();
                dict = dict.Where(kvp => kvp.Key != @"global" && kvp.Key != @"mania").ToArray();

                return dict.All(kvp => kvp.Value.Select(d => d.Type.Name).SequenceEqual([nameof(BigBlackBox), nameof(LegacyHealthDisplay)])) &&
                       dict.All(kvp => kvp.Value.Single(d => d.Type.Name == nameof(LegacyHealthDisplay)).Rotation == 0f);
            });
            AddAssert("no health display in mania HUD", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.MainHUDComponents].DrawableInfo;
                return dict.Single(kvp => kvp.Key == @"mania").Value.Single().Type.Name == nameof(BigBlackBox);
            });

            // Playfield
            AddAssert("playfield not configured", () => !skin.LayoutInfos.ContainsKey(GlobalSkinnableContainers.Playfield));
        }

        [Test]
        public void TestMigration_2_PlayfieldHasHealthDisplay()
        {
            LegacySkin skin = null!;

            AddStep("load skin", () =>
            {
                skin = loadSkin<LegacySkin>(new Dictionary<GlobalSkinnableContainers, SkinLayoutInfo>
                {
                    {
                        GlobalSkinnableContainers.Playfield,
                        createLayout(1, [], "mania", [nameof(LegacyHealthDisplay)])
                    }
                });
            });

            // HUD
            AddAssert("HUD not configured", () => !skin.LayoutInfos.ContainsKey(GlobalSkinnableContainers.MainHUDComponents));

            // Playfield
            AddAssert("no extra health display added", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.Playfield].DrawableInfo;
                return dict.Single(kvp => kvp.Key == @"mania").Value.Select(d => d.Type.Name).SequenceEqual([nameof(BigBlackBox), nameof(LegacyHealthDisplay)]);
            });
            AddAssert("rest is unaffected", () =>
            {
                var dict = skin.LayoutInfos[GlobalSkinnableContainers.Playfield].DrawableInfo;
                return dict.Where(kvp => kvp.Key != @"mania").All(d => d.Value.Single().Type.Name == nameof(BigBlackBox));
            });
        }

        #endregion

        private SkinLayoutInfo createLayout(int version, string[]? globalComponents = null, string? ruleset = null, string[]? rulesetComponents = null)
        {
            var info = new SkinLayoutInfo { Version = version };

            if (globalComponents != null)
                info.DrawableInfo.Add(@"global", globalComponents.Select(c => resolveComponent(c).CreateSerialisedInfo()).ToArray());

            if (ruleset != null && rulesetComponents != null)
                info.DrawableInfo.Add(ruleset, rulesetComponents.Select(c => resolveComponent(c).CreateSerialisedInfo()).ToArray());

            // add random drawable to ensure nothing is incorrectly discarded
            foreach (string key in rulesets.AvailableRulesets.Select(r => r.ShortName).Prepend(@"global"))
            {
                if (!info.DrawableInfo.TryGetValue(key, out var drawables))
                    info.DrawableInfo.Add(key, drawables = Array.Empty<SerialisedDrawableInfo>());

                info.DrawableInfo[key] = drawables.Prepend(new BigBlackBox().CreateSerialisedInfo()).ToArray();
            }

            return info;
        }

        private Drawable resolveComponent(string name, string? ruleset = null)
        {
            var drawables = SerialisedDrawableInfo.GetAllAvailableDrawables();

            if (ruleset != null)
                drawables = drawables.Concat(SerialisedDrawableInfo.GetAllAvailableDrawables(rulesets.GetRuleset(ruleset))).ToArray();

            return (Drawable)Activator.CreateInstance(drawables.Single(d => d.Name == name))!;
        }

        private T loadSkin<T>(IDictionary<GlobalSkinnableContainers, SkinLayoutInfo>? layout = null)
            where T : Skin
        {
            var info = new TestSkinInfo(typeof(T).GetInvariantInstantiationInfo(), layout);
            return (T)info.CreateInstance(new TestStorageResourceProvider(layout, info.Files, Realm));
        }

        private class TestSkinInfo : SkinInfo
        {
            public TestSkinInfo(string instantiationInfo, IDictionary<GlobalSkinnableContainers, SkinLayoutInfo>? layout)
                : base("test skin", "me", instantiationInfo)
            {
                if (layout != null)
                {
                    foreach (var kvp in layout)
                        Files.Add(new RealmNamedFileUsage(new RealmFile { Hash = Guid.NewGuid().ToString().ComputeMD5Hash() }, kvp.Key + ".json"));
                }
            }
        }

        private class TestStorageResourceProvider : IStorageResourceProvider
        {
            public IRenderer Renderer { get; } = new DummyRenderer();
            public IResourceStore<byte[]> Resources { get; } = new ResourceStore<byte[]>();
            public IResourceStore<TextureUpload>? CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => null;

            public AudioManager? AudioManager => null;

            public IResourceStore<byte[]> Files { get; }
            public RealmAccess RealmAccess { get; }

            public TestStorageResourceProvider(IDictionary<GlobalSkinnableContainers, SkinLayoutInfo>? layout, IList<RealmNamedFileUsage> files, RealmAccess realm)
            {
                Files = new TestResourceStore(layout, files);
                RealmAccess = realm;
            }

            private class TestResourceStore : ResourceStore<byte[]>
            {
                private readonly IDictionary<GlobalSkinnableContainers, SkinLayoutInfo>? layout;
                private readonly IList<RealmNamedFileUsage> files;

                public TestResourceStore(IDictionary<GlobalSkinnableContainers, SkinLayoutInfo>? layout, IList<RealmNamedFileUsage> files)
                {
                    this.layout = layout;
                    this.files = files;
                }

                public override byte[] Get(string name)
                {
                    string? filename = files.SingleOrDefault(f => f.File.GetStoragePath() == name)?.Filename;
                    if (filename == null || layout == null)
                        return base.Get(name);

                    if (!Enum.TryParse<GlobalSkinnableContainers>(filename.Replace(@".json", string.Empty), out var type) ||
                        !layout.TryGetValue(type, out var info))
                        return base.Get(name);

                    string json = JsonConvert.SerializeObject(info, new JsonSerializerSettings { Formatting = Formatting.Indented });
                    return Encoding.UTF8.GetBytes(json);
                }
            }
        }
    }
}
