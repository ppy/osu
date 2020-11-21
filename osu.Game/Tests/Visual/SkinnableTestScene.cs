// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public abstract class SkinnableTestScene : OsuGridTestScene
    {
        private Skin metricsSkin;
        private Skin defaultSkin;
        private Skin specialSkin;
        private Skin oldSkin;

        protected SkinnableTestScene()
            : base(2, 3)
        {
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, SkinManager skinManager, OsuGameBase game)
        {
            var dllStore = new DllResourceStore(DynamicCompilationOriginal.GetType().Assembly);

            metricsSkin = new TestLegacySkin(new SkinInfo { Name = "metrics-skin" }, new NamespacedResourceStore<byte[]>(dllStore, "Resources/metrics_skin"), audio, true);
            defaultSkin = new DefaultLegacySkin(new NamespacedResourceStore<byte[]>(game.Resources, "Skins/Legacy"), audio);
            specialSkin = new TestLegacySkin(new SkinInfo { Name = "special-skin" }, new NamespacedResourceStore<byte[]>(dllStore, "Resources/special_skin"), audio, true);
            oldSkin = new TestLegacySkin(new SkinInfo { Name = "old-skin" }, new NamespacedResourceStore<byte[]>(dllStore, "Resources/old_skin"), audio, true);
        }

        private readonly List<Drawable> createdDrawables = new List<Drawable>();

        public void SetContents(Func<Drawable> creationFunction)
        {
            createdDrawables.Clear();

            var beatmap = CreateBeatmapForSkinProvider();

            Cell(0).Child = createProvider(null, creationFunction, beatmap);
            Cell(1).Child = createProvider(metricsSkin, creationFunction, beatmap);
            Cell(2).Child = createProvider(defaultSkin, creationFunction, beatmap);
            Cell(3).Child = createProvider(specialSkin, creationFunction, beatmap);
            Cell(4).Child = createProvider(oldSkin, creationFunction, beatmap);
        }

        protected IEnumerable<Drawable> CreatedDrawables => createdDrawables;

        private Drawable createProvider(Skin skin, Func<Drawable> creationFunction, IBeatmap beatmap)
        {
            var created = creationFunction();

            createdDrawables.Add(created);

            SkinProvidingContainer mainProvider;
            Container childContainer;
            OutlineBox outlineBox;
            SkinProvidingContainer skinProvider;

            var children = new Container
            {
                RelativeSizeAxes = Axes.Both,
                BorderColour = Color4.White,
                BorderThickness = 5,
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
                        Text = skin?.SkinInfo?.Name ?? "none",
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
                            (mainProvider = new SkinProvidingContainer(skin)).WithChild(
                                skinProvider = new SkinProvidingContainer(Ruleset.Value.CreateInstance().CreateLegacySkinProvider(mainProvider, beatmap))
                                {
                                    Child = created,
                                }
                            )
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
                var autoSize = created.RelativeSizeAxes == Axes.None;

                foreach (var c in new[] { mainProvider, childContainer, skinProvider })
                {
                    c.RelativeSizeAxes = Axes.None;
                    c.AutoSizeAxes = Axes.None;

                    c.RelativeSizeAxes = !autoSize ? Axes.Both : Axes.None;
                    c.AutoSizeAxes = autoSize ? Axes.Both : Axes.None;
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

        private class OutlineBox : CompositeDrawable
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

            public TestLegacySkin(SkinInfo skin, IResourceStore<byte[]> storage, AudioManager audioManager, bool extrapolateAnimations)
                : base(skin, storage, audioManager, "skin.ini")
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

                    if (match.Length > 0 && int.TryParse(match.Groups[1].Value, out var number) && number < 60)
                        return base.GetTexture(componentName.Replace($"-{number}", $"-{number % 2}"), wrapModeS, wrapModeT);
                }

                return null;
            }
        }
    }
}
