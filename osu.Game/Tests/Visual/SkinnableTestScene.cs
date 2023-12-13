// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Sprites;
using osu.Game.IO;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public abstract partial class SkinnableTestScene : OsuGridTestScene, IStorageResourceProvider
    {
        private TrianglesSkin trianglesSkin;
        private Skin metricsSkin;
        private Skin legacySkin;
        private Skin argonSkin;
        private Skin specialSkin;
        private Skin oldSkin;

        [Resolved]
        private GameHost host { get; set; }

        protected SkinnableTestScene()
            : base(2, 3)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var dllStore = new DllResourceStore(GetType().Assembly);

            argonSkin = new ArgonSkin(this);
            trianglesSkin = new TrianglesSkin(this);
            metricsSkin = new TestLegacySkin(new SkinInfo { Name = "metrics-skin" }, new NamespacedResourceStore<byte[]>(dllStore, "Resources/metrics_skin"), this, true);
            legacySkin = new DefaultLegacySkin(this);
            specialSkin = new TestLegacySkin(new SkinInfo { Name = "special-skin" }, new NamespacedResourceStore<byte[]>(dllStore, "Resources/special_skin"), this, true);
            oldSkin = new TestLegacySkin(new SkinInfo { Name = "old-skin" }, new NamespacedResourceStore<byte[]>(dllStore, "Resources/old_skin"), this, true);
        }

        private readonly List<Drawable> createdDrawables = new List<Drawable>();

        protected void SetContents(Func<ISkin, Drawable> creationFunction)
        {
            createdDrawables.Clear();

            var beatmap = CreateBeatmapForSkinProvider();

            Cell(0).Child = createProvider(argonSkin, creationFunction, beatmap);
            Cell(1).Child = createProvider(trianglesSkin, creationFunction, beatmap);
            Cell(2).Child = createProvider(metricsSkin, creationFunction, beatmap);
            Cell(3).Child = createProvider(legacySkin, creationFunction, beatmap);
            Cell(4).Child = createProvider(specialSkin, creationFunction, beatmap);
            Cell(5).Child = createProvider(oldSkin, creationFunction, beatmap);
        }

        protected IEnumerable<Drawable> CreatedDrawables => createdDrawables;

        private Drawable createProvider(Skin skin, Func<ISkin, Drawable> creationFunction, IBeatmap beatmap)
        {
            var created = creationFunction(skin);

            createdDrawables.Add(created);

            Container childContainer;
            OutlineBox outlineBox;
            SkinProvidingContainer skinProvider;

            ISkin provider = Ruleset.Value.CreateInstance().CreateSkinTransformer(skin, beatmap) ?? skin;

            var children = new Container
            {
                RelativeSizeAxes = Axes.Both,
                BorderColour = Color4.White,
                BorderThickness = 3,
                Masking = true,

                Children = new Drawable[]
                {
                    new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new OsuSpriteText
                    {
                        Text = skin.SkinInfo.Value.Name,
                        Scale = new Vector2(1.5f),
                        Padding = new MarginPadding(5),
                    },
                    childContainer = new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            outlineBox = new OutlineBox(),
                            skinProvider = new SkinProvidingContainer(provider)
                            {
                                Child = created,
                            }
                        }
                    },
                }
            };

            // run this once initially to bring things into a sane state as early as possible.
            updateSizing();

            // run this once after construction to handle the case the changes are made in a BDL/LoadComplete call.
            Schedule(updateSizing);

            return children;

            void updateSizing()
            {
                bool autoSize = created.RelativeSizeAxes == Axes.None;

                foreach (var c in new[] { childContainer, skinProvider })
                {
                    c.RelativeSizeAxes = Axes.None;
                    c.AutoSizeAxes = Axes.None;
                    c.Size = Vector2.Zero;

                    if (autoSize)
                        c.AutoSizeAxes = Axes.Both;
                    else
                    {
                        c.RelativeSizeAxes = Axes.Both;
                        c.Anchor = Anchor.Centre;
                        c.Origin = Anchor.Centre;
                        c.Size = new Vector2(0.97f);
                    }
                }

                outlineBox.Alpha = autoSize ? 1 : 0;
            }
        }

        /// <summary>
        /// Creates the ruleset for adding the corresponding skin transforming component.
        /// </summary>
        [NotNull]
        protected abstract Ruleset CreateRulesetForSkinProvider();

        protected sealed override Ruleset CreateRuleset() => CreateRulesetForSkinProvider();

        protected virtual IBeatmap CreateBeatmapForSkinProvider() => CreateWorkingBeatmap(Ruleset.Value).GetPlayableBeatmap(Ruleset.Value);

        #region IResourceStorageProvider

        public IRenderer Renderer => host.Renderer;
        public AudioManager AudioManager => Audio;
        public IResourceStore<byte[]> Files => null;
        public new IResourceStore<byte[]> Resources => base.Resources;
        public IResourceStore<TextureUpload> CreateTextureLoaderStore(IResourceStore<byte[]> underlyingStore) => host.CreateTextureLoaderStore(underlyingStore);
        RealmAccess IStorageResourceProvider.RealmAccess => null;

        #endregion

        private partial class OutlineBox : CompositeDrawable
        {
            public OutlineBox()
            {
                BorderColour = Color4.IndianRed;
                BorderThickness = 5;
                Masking = true;
                RelativeSizeAxes = Axes.Both;

                InternalChild = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Colour = Color4.Brown,
                    AlwaysPresent = true
                };
            }
        }

        private class TestLegacySkin : LegacySkin
        {
            private readonly bool extrapolateAnimations;

            public TestLegacySkin(SkinInfo skin, IResourceStore<byte[]> fallbackStore, IStorageResourceProvider resources, bool extrapolateAnimations)
                : base(skin, resources, fallbackStore)
            {
                this.extrapolateAnimations = extrapolateAnimations;
            }

            public override Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            {
                var lookup = base.GetTexture(componentName, wrapModeS, wrapModeT);

                if (lookup != null)
                    return lookup;

                // extrapolate frames to test longer animations
                if (extrapolateAnimations)
                {
                    var match = Regex.Match(componentName, "-([0-9]*)");

                    if (match.Length > 0 && int.TryParse(match.Groups[1].Value, out int number) && number < 60)
                        return base.GetTexture(componentName.Replace($"-{number}", $"-{number % 2}"), wrapModeS, wrapModeT);
                }

                return null;
            }
        }
    }
}
