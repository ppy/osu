// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using System.Linq;
using osu.Game.Audio;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Objects;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract class DrawableTaikoHitObject : DrawableHitObject<TaikoHitObject>, IKeyBindingHandler<TaikoAction>
    {
        protected readonly Container Content;
        private readonly Container proxiedContent;

        private readonly Container nonProxiedContent;

        protected DrawableTaikoHitObject(TaikoHitObject hitObject)
            : base(hitObject)
        {
            AddRangeInternal(new[]
            {
                nonProxiedContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = Content = new Container { RelativeSizeAxes = Axes.Both }
                },
                proxiedContent = new ProxiedContentContainer { RelativeSizeAxes = Axes.Both }
            });
        }

        /// <summary>
        /// <see cref="proxiedContent"/> is proxied into an upper layer. We don't want to get masked away otherwise <see cref="proxiedContent"/> would too.
        /// </summary>
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        private bool isProxied;

        /// <summary>
        /// Moves <see cref="Content"/> to a layer proxied above the playfield.
        /// Does nothing if content is already proxied.
        /// </summary>
        protected void ProxyContent()
        {
            if (isProxied) return;

            isProxied = true;

            nonProxiedContent.Remove(Content);
            proxiedContent.Add(Content);
        }

        /// <summary>
        /// Moves <see cref="Content"/> to the normal hitobject layer.
        /// Does nothing is content is not currently proxied.
        /// </summary>
        protected void UnproxyContent()
        {
            if (!isProxied) return;

            isProxied = false;

            proxiedContent.Remove(Content);
            nonProxiedContent.Add(Content);
        }

        /// <summary>
        /// Creates a proxy for the content of this <see cref="DrawableTaikoHitObject"/>.
        /// </summary>
        public Drawable CreateProxiedContent() => proxiedContent.CreateProxy();

        public abstract bool OnPressed(TaikoAction action);

        public virtual void OnReleased(TaikoAction action)
        {
        }

        public override double LifetimeStart
        {
            get => base.LifetimeStart;
            set
            {
                base.LifetimeStart = value;
                proxiedContent.LifetimeStart = value;
            }
        }

        public override double LifetimeEnd
        {
            get => base.LifetimeEnd;
            set
            {
                base.LifetimeEnd = value;
                proxiedContent.LifetimeEnd = value;
            }
        }

        private class ProxiedContentContainer : Container
        {
            public override bool RemoveWhenNotAlive => false;
        }
    }

    public abstract class DrawableTaikoHitObject<TObject> : DrawableTaikoHitObject
        where TObject : TaikoHitObject
    {
        public override Vector2 OriginPosition => new Vector2(DrawHeight / 2);

        public new TObject HitObject;

        protected readonly Vector2 BaseSize;
        protected readonly SkinnableDrawable MainPiece;

        private readonly Container<DrawableStrongNestedHit> strongHitContainer;

        protected DrawableTaikoHitObject(TObject hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Custom;

            RelativeSizeAxes = Axes.Both;
            Size = BaseSize = new Vector2(HitObject.IsStrong ? TaikoHitObject.DEFAULT_STRONG_SIZE : TaikoHitObject.DEFAULT_SIZE);

            Content.Add(MainPiece = CreateMainPiece());

            AddInternal(strongHitContainer = new Container<DrawableStrongNestedHit>());
        }

        protected override void AddNestedHitObject(DrawableHitObject hitObject)
        {
            base.AddNestedHitObject(hitObject);

            switch (hitObject)
            {
                case DrawableStrongNestedHit strong:
                    strongHitContainer.Add(strong);
                    break;
            }
        }

        protected override void ClearNestedHitObjects()
        {
            base.ClearNestedHitObjects();
            strongHitContainer.Clear();
        }

        protected override DrawableHitObject CreateNestedHitObject(HitObject hitObject)
        {
            switch (hitObject)
            {
                case StrongHitObject strong:
                    return CreateStrongHit(strong);
            }

            return base.CreateNestedHitObject(hitObject);
        }

        // Normal and clap samples are handled by the drum
        protected override IEnumerable<HitSampleInfo> GetSamples() => HitObject.Samples.Where(s => s.Name != HitSampleInfo.HIT_NORMAL && s.Name != HitSampleInfo.HIT_CLAP);

        protected abstract SkinnableDrawable CreateMainPiece();

        /// <summary>
        /// Creates the handler for this <see cref="DrawableHitObject"/>'s <see cref="StrongHitObject"/>.
        /// This is only invoked if <see cref="TaikoHitObject.IsStrong"/> is true for <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The strong hitobject.</param>
        /// <returns>The strong hitobject handler.</returns>
        protected virtual DrawableStrongNestedHit CreateStrongHit(StrongHitObject hitObject) => null;
    }
}
