// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Types;
using OpenTK.Graphics;

namespace osu.Game.Skinning
{
    public class LocalSkinOverrideContainer : Container, ISkinSource
    {
        public event Action SourceChanged;

        public Drawable GetDrawableComponent(string componentName) => source.GetDrawableComponent(componentName) ?? fallbackSource?.GetDrawableComponent(componentName);

        public SampleChannel GetSample(string sampleName) => source.GetSample(sampleName) ?? fallbackSource?.GetSample(sampleName);

        public Color4? GetComboColour(IHasComboIndex comboObject) => source.GetComboColour(comboObject) ?? fallbackSource?.GetComboColour(comboObject);

        private readonly ISkinSource source;
        private ISkinSource fallbackSource;

        public LocalSkinOverrideContainer(ISkinSource source)
        {
            this.source = source;
        }

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

            fallbackSource = dependencies.Get<ISkinSource>();
            if (fallbackSource != null)
                fallbackSource.SourceChanged += () => SourceChanged?.Invoke();

            dependencies.CacheAs<ISkinSource>(this);

            return dependencies;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (fallbackSource != null)
                fallbackSource.SourceChanged -= SourceChanged;
        }
    }
}
