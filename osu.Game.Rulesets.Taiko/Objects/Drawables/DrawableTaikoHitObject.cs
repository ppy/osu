// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using OpenTK;
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
        public readonly Container ProxiedContent;

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
                ProxiedContent = new Container { RelativeSizeAxes = Axes.Both }
            };
        }

        /// <summary>
        /// <see cref="ProxiedContent"/> is proxied into an upper layer. We don't want to get masked away otherwise <see cref="ProxiedContent"/> would too.
        /// </summary>
        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        /// <summary>
        /// Moves <see cref="Content"/> to a layer proxied above the playfield.
        /// </summary>
        protected void ProxyContent()
        {
            nonProxiedContent.Remove(Content);
            ProxiedContent.Remove(Content);
            ProxiedContent.Add(Content);
        }

        /// <summary>
        /// Moves <see cref="Content"/> to the normal hitobject layer.
        /// </summary>
        protected void UnproxyContent()
        {
            ProxiedContent.Remove(Content);
            nonProxiedContent.Remove(Content);
            nonProxiedContent.Add(Content);
        }

        public abstract bool OnPressed(TaikoAction action);
        public virtual bool OnReleased(TaikoAction action) => false;
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
        }

        // Normal and clap samples are handled by the drum
        protected override IEnumerable<SampleInfo> GetSamples() => HitObject.Samples.Where(s => s.Name != SampleInfo.HIT_NORMAL && s.Name != SampleInfo.HIT_CLAP);

        protected override string SampleNamespace => "Taiko";

        protected virtual TaikoPiece CreateMainPiece() => new CirclePiece();
    }
}
