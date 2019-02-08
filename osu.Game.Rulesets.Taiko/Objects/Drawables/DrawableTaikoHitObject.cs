﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using osuTK;
using System.Linq;
using osu.Game.Audio;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;

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
            InternalChildren = new[]
            {
                nonProxiedContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = Content = new Container { RelativeSizeAxes = Axes.Both }
                },
                proxiedContent = new ProxiedContentContainer { RelativeSizeAxes = Axes.Both }
            };
        }

        /// <summary>
        /// <see cref="proxiedContent"/> is proxied into an upper layer. We don't want to get masked away otherwise <see cref="proxiedContent"/> would too.
        /// </summary>
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        private bool isProxied;

        /// <summary>
        /// Moves <see cref="Content"/> to a layer proxied above the playfield.
        /// Does nothing is content is already proxied.
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
        public virtual bool OnReleased(TaikoAction action) => false;

        private class ProxiedContentContainer : Container
        {
            public override double LifetimeStart => Parent?.LifetimeStart ?? base.LifetimeStart;
            public override double LifetimeEnd => Parent?.LifetimeEnd ?? base.LifetimeEnd;
        }
    }

    public abstract class DrawableTaikoHitObject<TaikoHitType> : DrawableTaikoHitObject
        where TaikoHitType : TaikoHitObject
    {
        public override Vector2 OriginPosition => new Vector2(DrawHeight / 2);

        protected readonly Vector2 BaseSize;

        protected readonly TaikoPiece MainPiece;

        public new TaikoHitType HitObject;

        protected DrawableTaikoHitObject(TaikoHitType hitObject)
            : base(hitObject)
        {
            HitObject = hitObject;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Custom;

            RelativeSizeAxes = Axes.Both;
            Size = BaseSize = new Vector2(HitObject.IsStrong ? TaikoHitObject.DEFAULT_STRONG_SIZE : TaikoHitObject.DEFAULT_SIZE);

            Content.Add(MainPiece = CreateMainPiece());
            MainPiece.KiaiMode = HitObject.Kiai;

            var strongObject = HitObject.NestedHitObjects.OfType<StrongHitObject>().FirstOrDefault();
            if (strongObject != null)
            {
                var strongHit = CreateStrongHit(strongObject);

                AddNested(strongHit);
                AddInternal(strongHit);
            }
        }

        // Normal and clap samples are handled by the drum
        protected override IEnumerable<SampleInfo> GetSamples() => HitObject.Samples.Where(s => s.Name != SampleInfo.HIT_NORMAL && s.Name != SampleInfo.HIT_CLAP);

        protected override string SampleNamespace => "Taiko";

        protected virtual TaikoPiece CreateMainPiece() => new CirclePiece();

        /// <summary>
        /// Creates the handler for this <see cref="DrawableHitObject"/>'s <see cref="StrongHitObject"/>.
        /// This is only invoked if <see cref="TaikoHitObject.IsStrong"/> is true for <see cref="HitObject"/>.
        /// </summary>
        /// <param name="hitObject">The strong hitobject.</param>
        /// <returns>The strong hitobject handler.</returns>
        protected virtual DrawableStrongNestedHit CreateStrongHit(StrongHitObject hitObject) => null;
    }
}
