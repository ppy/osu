// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Textures;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Rulesets
{
    [HeadlessTest]
    public class TestSceneDrawableRulesetDependencies : OsuTestScene
    {
        [Test]
        public void TestDisposalDoesNotDisposeParentStores()
        {
            DrawableWithDependencies drawable = null;
            TestTextureStore textureStore = null;
            TestSampleStore sampleStore = null;

            AddStep("add dependencies", () =>
            {
                Child = drawable = new DrawableWithDependencies();
                textureStore = drawable.ParentTextureStore;
                sampleStore = drawable.ParentSampleStore;
            });

            AddStep("clear children", Clear);
            AddUntilStep("wait for disposal", () => drawable.IsDisposed);

            AddStep("GC", () =>
            {
                drawable = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            });

            AddAssert("parent texture store not disposed", () => !textureStore.IsDisposed);
            AddAssert("parent sample store not disposed", () => !sampleStore.IsDisposed);
        }

        private class DrawableWithDependencies : CompositeDrawable
        {
            public TestTextureStore ParentTextureStore { get; private set; }
            public TestSampleStore ParentSampleStore { get; private set; }

            public DrawableWithDependencies()
            {
                InternalChild = new Box { RelativeSizeAxes = Axes.Both };
            }

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
            {
                var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

                dependencies.CacheAs<TextureStore>(ParentTextureStore = new TestTextureStore());
                dependencies.CacheAs<ISampleStore>(ParentSampleStore = new TestSampleStore());

                return new DrawableRulesetDependencies(new OsuRuleset(), dependencies);
            }

            public new bool IsDisposed { get; private set; }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                IsDisposed = true;
            }
        }

        private class TestTextureStore : TextureStore
        {
            public override Texture Get(string name, WrapMode wrapModeS, WrapMode wrapModeT) => null;

            public bool IsDisposed { get; private set; }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                IsDisposed = true;
            }
        }

        private class TestSampleStore : ISampleStore
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }

            public Sample Get(string name) => null;

            public Task<Sample> GetAsync(string name) => null;

            public Stream GetStream(string name) => null;

            public IEnumerable<string> GetAvailableResources() => throw new NotImplementedException();

            public BindableNumber<double> Volume => throw new NotImplementedException();
            public BindableNumber<double> Balance => throw new NotImplementedException();
            public BindableNumber<double> Frequency => throw new NotImplementedException();
            public BindableNumber<double> Tempo => throw new NotImplementedException();

            public void BindAdjustments(IAggregateAudioAdjustment component) => throw new NotImplementedException();

            public void UnbindAdjustments(IAggregateAudioAdjustment component) => throw new NotImplementedException();

            public void AddAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => throw new NotImplementedException();

            public void RemoveAdjustment(AdjustableProperty type, IBindable<double> adjustBindable) => throw new NotImplementedException();

            public void RemoveAllAdjustments(AdjustableProperty type) => throw new NotImplementedException();

            public IBindable<double> AggregateVolume => throw new NotImplementedException();
            public IBindable<double> AggregateBalance => throw new NotImplementedException();
            public IBindable<double> AggregateFrequency => throw new NotImplementedException();
            public IBindable<double> AggregateTempo => throw new NotImplementedException();

            public int PlaybackConcurrency { get; set; }
        }
    }
}
