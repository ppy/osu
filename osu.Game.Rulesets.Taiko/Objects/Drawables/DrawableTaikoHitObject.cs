// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract partial class DrawableTaikoHitObject : DrawableHitObject<TaikoHitObject>, IKeyBindingHandler<TaikoAction>
    {
        protected readonly Container Content;
        private readonly Container proxiedContent;

        private readonly Container nonProxiedContent;

        /// <summary>
        /// Whether the location of the hit should be snapped to the hit target before animating.
        /// </summary>
        /// <remarks>
        /// This is how osu-stable worked, but notably is not how TnT works.
        /// Not snapping results in less visual feedback on hit accuracy.
        /// </remarks>
        public bool SnapJudgementLocation { get; set; }

        protected DrawableTaikoHitObject([CanBeNull] TaikoHitObject hitObject)
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

            nonProxiedContent.Remove(Content, false);
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

            proxiedContent.Remove(Content, false);
            nonProxiedContent.Add(Content);
        }

        /// <summary>
        /// Creates a proxy for the content of this <see cref="DrawableTaikoHitObject"/>.
        /// </summary>
        public Drawable CreateProxiedContent() => proxiedContent.CreateProxy();

        public abstract bool OnPressed(KeyBindingPressEvent<TaikoAction> e);

        public virtual void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
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

        private partial class ProxiedContentContainer : Container
        {
            public override bool RemoveWhenNotAlive => false;
        }

        // osu!taiko hitsounds are managed by the drum (see DrumSampleTriggerSource).
        public sealed override IEnumerable<HitSampleInfo> GetSamples() => Enumerable.Empty<HitSampleInfo>();
    }

    public abstract partial class DrawableTaikoHitObject<TObject> : DrawableTaikoHitObject
        where TObject : TaikoHitObject
    {
        public override Vector2 OriginPosition => new Vector2(DrawHeight / 2);

        public new TObject HitObject => (TObject)base.HitObject;

        protected Vector2 BaseSize;
        protected SkinnableDrawable MainPiece;

        protected DrawableTaikoHitObject([CanBeNull] TObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Custom;

            RelativeSizeAxes = Axes.Both;
        }

        protected override void OnApply()
        {
            base.OnApply();

            // TODO: THIS CANNOT BE HERE, it makes pooling pointless (see https://github.com/ppy/osu/issues/21072).
            RecreatePieces();
        }

        protected virtual void RecreatePieces()
        {
            Size = BaseSize = new Vector2(TaikoHitObject.DEFAULT_SIZE);

            if (MainPiece != null)
                Content.Remove(MainPiece, true);

            Content.Add(MainPiece = CreateMainPiece());
        }

        protected abstract SkinnableDrawable CreateMainPiece();
    }
}
