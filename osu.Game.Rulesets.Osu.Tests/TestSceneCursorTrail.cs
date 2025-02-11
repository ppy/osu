// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Framework.Testing.Input;
using osu.Game.Audio;
using osu.Game.Rulesets.Osu.Skinning;
using osu.Game.Rulesets.Osu.Skinning.Legacy;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Skinning;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneCursorTrail : OsuTestScene
    {
        [Resolved]
        private IRenderer renderer { get; set; }

        [Test]
        public void TestSmoothCursorTrail()
        {
            Container scalingContainer = null;

            createTest(() => scalingContainer = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = new CursorTrail()
            });

            AddStep("set large scale", () => scalingContainer.Scale = new Vector2(10));
        }

        [Test]
        public void TestLegacySmoothCursorTrail()
        {
            createTest(() =>
            {
                var skinContainer = new LegacySkinContainer(renderer, provideMiddle: false);
                var legacyCursorTrail = new LegacyCursorTrail(skinContainer);

                skinContainer.Child = legacyCursorTrail;

                return skinContainer;
            });
        }

        [Test]
        public void TestLegacyDisjointCursorTrail()
        {
            createTest(() =>
            {
                var skinContainer = new LegacySkinContainer(renderer, provideMiddle: true);
                var legacyCursorTrail = new LegacyCursorTrail(skinContainer);

                skinContainer.Child = legacyCursorTrail;

                return skinContainer;
            });
        }

        [Test]
        public void TestLegacyDisjointCursorTrailViaNoCursor()
        {
            createTest(() =>
            {
                var skinContainer = new LegacySkinContainer(renderer, provideMiddle: false, provideCursor: false);
                var legacyCursorTrail = new LegacyCursorTrail(skinContainer);

                skinContainer.Child = legacyCursorTrail;

                return skinContainer;
            });

            AddAssert("trail is disjoint", () => this.ChildrenOfType<LegacyCursorTrail>().Single().DisjointTrail, () => Is.True);
        }

        [Test]
        public void TestClickExpand()
        {
            createTest(() => new Container
            {
                RelativeSizeAxes = Axes.Both,
                Scale = new Vector2(10),
                Child = new CursorTrail(),
            });

            AddStep("expand", () => this.ChildrenOfType<CursorTrail>().Single().NewPartScale = new Vector2(3));
            AddWaitStep("let the cursor trail draw a bit", 5);
            AddStep("contract", () => this.ChildrenOfType<CursorTrail>().Single().NewPartScale = Vector2.One);
        }

        [Test]
        public void TestRotation()
        {
            createTest(() =>
            {
                var skinContainer = new LegacySkinContainer(renderer, provideMiddle: true, enableRotation: true);
                var legacyCursorTrail = new LegacyRotatingCursorTrail(skinContainer)
                {
                    NewPartScale = new Vector2(10)
                };

                skinContainer.Child = legacyCursorTrail;

                return skinContainer;
            });
        }

        private void createTest(Func<Drawable> createContent) => AddStep("create trail", () =>
        {
            Clear();

            Add(new Container
            {
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.8f),
                Child = new MovingCursorInputManager { Child = createContent() }
            });
        });

        [Cached(typeof(ISkinSource))]
        private partial class LegacySkinContainer : Container, ISkinSource
        {
            private readonly IRenderer renderer;
            private readonly bool provideMiddle;
            private readonly bool provideCursor;
            private readonly bool enableRotation;

            public LegacySkinContainer(IRenderer renderer, bool provideMiddle, bool provideCursor = true, bool enableRotation = false)
            {
                this.renderer = renderer;
                this.provideMiddle = provideMiddle;
                this.provideCursor = provideCursor;
                this.enableRotation = enableRotation;

                RelativeSizeAxes = Axes.Both;
            }

            public Drawable GetDrawableComponent(ISkinComponentLookup lookup) => null;

            public Texture GetTexture(string componentName, WrapMode wrapModeS, WrapMode wrapModeT)
            {
                switch (componentName)
                {
                    case "cursor":
                        return provideCursor ? new Texture(renderer.WhitePixel) : null;

                    case "cursortrail":
                        return new Texture(renderer.WhitePixel);

                    case "cursormiddle":
                        return provideMiddle ? null : renderer.WhitePixel;
                }

                return null;
            }

            public ISample GetSample(ISampleInfo sampleInfo) => null;

            public IBindable<TValue> GetConfig<TLookup, TValue>(TLookup lookup)
            {
                switch (lookup)
                {
                    case OsuSkinConfiguration osuLookup:
                        if (osuLookup == OsuSkinConfiguration.CursorTrailRotate)
                            return SkinUtils.As<TValue>(new BindableBool(enableRotation));

                        break;
                }

                return null;
            }

            public ISkin FindProvider(Func<ISkin, bool> lookupFunction) => lookupFunction(this) ? this : null;

            public IEnumerable<ISkin> AllSources => new[] { this };

            public event Action SourceChanged
            {
                add { }
                remove { }
            }
        }

        private partial class MovingCursorInputManager : ManualInputManager
        {
            public MovingCursorInputManager()
            {
                UseParentInput = false;
            }

            protected override void Update()
            {
                base.Update();

                const double spin_duration = 1000;
                double currentTime = Time.Current;

                double angle = (currentTime % spin_duration) / spin_duration * 2 * Math.PI;
                Vector2 rPos = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                MoveMouseTo(ToScreenSpace(DrawSize / 2 + DrawSize / 3 * rPos));
            }
        }

        private partial class LegacyRotatingCursorTrail : LegacyCursorTrail
        {
            public LegacyRotatingCursorTrail([NotNull] ISkin skin)
                : base(skin)
            {
            }

            protected override void Update()
            {
                base.Update();
                PartRotation += (float)(Time.Elapsed * 0.1);
            }
        }
    }
}
