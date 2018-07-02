// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.UI.Components;
using osu.Game.Rulesets.UI.Scrolling;

namespace osu.Game.Rulesets.Mania.UI
{
    public class Column : ManiaScrollingPlayfield, IKeyBindingHandler<ManiaAction>, IHasAccentColour
    {
        private const float column_width = 45;
        private const float special_column_width = 70;

        public readonly Bindable<ManiaAction> Action = new Bindable<ManiaAction>();

        private readonly ColumnBackground background;
        private readonly ColumnKeyArea keyArea;
        private readonly ColumnHitObjectArea hitObjectArea;

        internal readonly Container TopLevelContainer;
        private readonly Container explosionContainer;

        protected override Container<Drawable> Content => hitObjectArea;

        public Column(ScrollingDirection direction)
            : base(direction)
        {
            RelativeSizeAxes = Axes.Y;
            Width = column_width;

            Masking = true;
            CornerRadius = 5;

            background = new ColumnBackground { RelativeSizeAxes = Axes.Both };

            Container hitTargetContainer;

            InternalChildren = new[]
            {
                // For input purposes, the background is added at the highest depth, but is then proxied back below all other elements
                background.CreateProxy(),
                hitTargetContainer = new Container
                {
                    Name = "Hit target + hit objects",
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        hitObjectArea = new ColumnHitObjectArea { RelativeSizeAxes = Axes.Both },
                        explosionContainer = new Container
                        {
                            Name = "Hit explosions",
                            RelativeSizeAxes = Axes.Both
                        }
                    }
                },
                keyArea = new ColumnKeyArea
                {
                    RelativeSizeAxes = Axes.X,
                    Height = ManiaStage.HIT_TARGET_POSITION,
                },
                background,
                TopLevelContainer = new Container { RelativeSizeAxes = Axes.Both }
            };

            TopLevelContainer.Add(explosionContainer.CreateProxy());

            Direction.BindValueChanged(d =>
            {
                hitTargetContainer.Padding = new MarginPadding
                {
                    Top = d == ScrollingDirection.Up ? ManiaStage.HIT_TARGET_POSITION : 0,
                    Bottom = d == ScrollingDirection.Down ? ManiaStage.HIT_TARGET_POSITION : 0,
                };

                keyArea.Anchor = keyArea.Origin= d == ScrollingDirection.Up ? Anchor.TopLeft : Anchor.BottomLeft;
            }, true);
        }

        public override Axes RelativeSizeAxes => Axes.Y;

        private bool isSpecial;
        public bool IsSpecial
        {
            get { return isSpecial; }
            set
            {
                if (isSpecial == value)
                    return;
                isSpecial = value;

                Width = isSpecial ? special_column_width : column_width;
            }
        }

        private Color4 accentColour;
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                if (accentColour == value)
                    return;
                accentColour = value;

                background.AccentColour = value;
                keyArea.AccentColour = value;
                hitObjectArea.AccentColour = value;
            }
        }

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));
            dependencies.CacheAs<IBindable<ManiaAction>>(Action);
            return dependencies;
        }

        /// <summary>
        /// Adds a DrawableHitObject to this Playfield.
        /// </summary>
        /// <param name="hitObject">The DrawableHitObject to add.</param>
        public override void Add(DrawableHitObject hitObject)
        {
            hitObject.AccentColour = AccentColour;
            hitObject.OnJudgement += OnJudgement;

            HitObjects.Add(hitObject);
        }

        internal void OnJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            if (!judgement.IsHit || !judgedObject.DisplayJudgement)
                return;

            explosionContainer.Add(new HitExplosion(judgedObject)
            {
                Anchor = Direction == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre
            });
        }

        public bool OnPressed(ManiaAction action)
        {
            if (action != Action)
                return false;

            var nextObject =
                HitObjects.AliveObjects.FirstOrDefault(h => h.HitObject.StartTime > Time.Current) ??
                // fallback to non-alive objects to find next off-screen object
                HitObjects.Objects.FirstOrDefault(h => h.HitObject.StartTime > Time.Current) ??
                HitObjects.Objects.LastOrDefault();

            nextObject?.PlaySamples();

            return true;
        }

        public bool OnReleased(ManiaAction action) => false;
    }
}
