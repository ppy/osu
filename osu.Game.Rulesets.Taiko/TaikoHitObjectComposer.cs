// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoHitObjectComposer : HitObjectComposer<TaikoHitObject>
    {
        private DrawableTaikoRuleset drawableRuleset;

        public TaikoHitObjectComposer(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new[]
        {
            new HitCompositionTool()
        };

        protected override ComposeBlueprintContainer CreateBlueprintContainer() => new TaikoBlueprintContainer(drawableRuleset.Playfield.AllHitObjects);

        protected override DrawableRuleset<TaikoHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
        {
            return drawableRuleset = new DrawableTaikoRuleset(ruleset, beatmap, mods);
        }
    }

    public class TaikoBlueprintContainer : ComposeBlueprintContainer
    {
        public TaikoBlueprintContainer(IEnumerable<DrawableHitObject> hitObjects)
            : base(hitObjects)
        {
        }

        protected override SelectionHandler CreateSelectionHandler() => new TaikoSelectionHandler();

        public override OverlaySelectionBlueprint CreateBlueprintFor(DrawableHitObject hitObject) =>
            new TaikoSelectionBlueprint(hitObject);
    }

    public class TaikoSelectionHandler : SelectionHandler
    {
        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint> selection)
        {
            if (selection.All(s => s.HitObject is Hit))
            {
                var hits = selection.Select(s => s.HitObject).OfType<Hit>();

                yield return new TernaryStateMenuItem("Rim", action: state =>
                {
                    foreach (var h in hits)
                    {
                        switch (state)
                        {
                            case TernaryState.True:
                                h.Type = HitType.Rim;
                                break;

                            case TernaryState.False:
                                h.Type = HitType.Centre;
                                break;
                        }
                    }
                })
                {
                    State = { Value = getTernaryState(hits, h => h.Type == HitType.Rim) }
                };
            }

            if (selection.All(s => s.HitObject is TaikoHitObject))
            {
                var hits = selection.Select(s => s.HitObject).OfType<TaikoHitObject>();

                yield return new TernaryStateMenuItem("Strong", action: state =>
                {
                    foreach (var h in hits)
                    {
                        switch (state)
                        {
                            case TernaryState.True:
                                h.IsStrong = true;
                                break;

                            case TernaryState.False:
                                h.IsStrong = false;
                                break;
                        }

                        EditorBeatmap?.UpdateHitObject(h);
                    }
                })
                {
                    State = { Value = getTernaryState(hits, h => h.IsStrong) }
                };
            }
        }

        private TernaryState getTernaryState<T>(IEnumerable<T> selection, Func<T, bool> func)
        {
            if (selection.Any(func))
                return selection.All(func) ? TernaryState.True : TernaryState.Indeterminate;

            return TernaryState.False;
        }
    }

    public class TaikoSelectionBlueprint : OverlaySelectionBlueprint
    {
        public TaikoSelectionBlueprint(DrawableHitObject hitObject)
            : base(hitObject)
        {
            RelativeSizeAxes = Axes.None;

            AddInternal(new HitPiece
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.TopLeft
            });
        }

        protected override void Update()
        {
            base.Update();

            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            topLeft = Vector2.ComponentMin(topLeft, Parent.ToLocalSpace(DrawableObject.ScreenSpaceDrawQuad.TopLeft));
            bottomRight = Vector2.ComponentMax(bottomRight, Parent.ToLocalSpace(DrawableObject.ScreenSpaceDrawQuad.BottomRight));

            Size = bottomRight - topLeft;
            Position = topLeft;
        }
    }

    public class HitCompositionTool : HitObjectCompositionTool
    {
        public HitCompositionTool()
            : base(nameof(Hit))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new HitPlacementBlueprint();
    }

    public class HitPlacementBlueprint : PlacementBlueprint

    {
        private readonly HitPiece piece;

        private static Hit hit;

        public HitPlacementBlueprint()
            : base(hit = new Hit())
        {
            InternalChild = piece = new HitPiece
            {
                Size = new Vector2(TaikoHitObject.DEFAULT_SIZE * TaikoPlayfield.DEFAULT_HEIGHT)
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Left:
                    hit.Type = HitType.Centre;
                    EndPlacement(true);
                    return true;

                case MouseButton.Right:
                    hit.Type = HitType.Rim;
                    EndPlacement(true);
                    return true;
            }

            return false;
        }

        public override void UpdatePosition(SnapResult snapResult)
        {
            piece.Position = ToLocalSpace(snapResult.ScreenSpacePosition);
            base.UpdatePosition(snapResult);
        }
    }

    public class HitPiece : CompositeDrawable
    {
        public HitPiece()
        {
            Origin = Anchor.Centre;

            InternalChild = new CircularContainer
            {
                Masking = true,
                BorderThickness = 10,
                BorderColour = Color4.Yellow,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        AlwaysPresent = true,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            };
        }
    }
}
