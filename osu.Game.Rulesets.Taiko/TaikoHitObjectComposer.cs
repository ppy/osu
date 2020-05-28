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
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
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

        protected override IReadOnlyList<HitObjectCompositionTool> CompositionTools => new HitObjectCompositionTool[]
        {
            new HitCompositionTool(),
            new DrumRollCompositionTool(),
            new SwellCompositionTool()
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

    public class SwellCompositionTool : HitObjectCompositionTool
    {
        public SwellCompositionTool()
            : base(nameof(Swell))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new SwellPlacementBlueprint();
    }

    public class DrumRollCompositionTool : HitObjectCompositionTool
    {
        public DrumRollCompositionTool()
            : base(nameof(DrumRoll))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new DrumRollPlacementBlueprint();
    }

    public class SwellPlacementBlueprint : TaikoSpanPlacementBlueprint
    {
        public SwellPlacementBlueprint()
            : base(new Swell())
        {
        }
    }

    public class DrumRollPlacementBlueprint : TaikoSpanPlacementBlueprint
    {
        public DrumRollPlacementBlueprint()
            : base(new DrumRoll())
        {
        }
    }

    public class TaikoSpanPlacementBlueprint : PlacementBlueprint
    {
        private readonly HitPiece headPiece;
        private readonly HitPiece tailPiece;

        private readonly LengthPiece lengthPiece;

        private readonly IHasDuration spanPlacementObject;

        public TaikoSpanPlacementBlueprint(HitObject hitObject)
            : base(hitObject)

        {
            spanPlacementObject = hitObject as IHasDuration;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                headPiece = new HitPiece
                {
                    Size = new Vector2(TaikoHitObject.DEFAULT_SIZE * TaikoPlayfield.DEFAULT_HEIGHT)
                },
                lengthPiece = new LengthPiece
                {
                    Height = TaikoHitObject.DEFAULT_SIZE * TaikoPlayfield.DEFAULT_HEIGHT
                },
                tailPiece = new HitPiece
                {
                    Size = new Vector2(TaikoHitObject.DEFAULT_SIZE * TaikoPlayfield.DEFAULT_HEIGHT)
                }
            };
        }

        private double originalStartTime;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return false;

            BeginPlacement(true);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button != MouseButton.Left)
                return;

            base.OnMouseUp(e);
            EndPlacement(true);
        }

        public override void UpdatePosition(SnapResult result)
        {
            base.UpdatePosition(result);

            if (PlacementActive)
            {
                if (result.Time is double endTime)
                {
                    if (endTime < originalStartTime)
                    {
                        HitObject.StartTime = endTime;
                        spanPlacementObject.Duration = Math.Abs(endTime - originalStartTime);
                        headPiece.Position = ToLocalSpace(result.ScreenSpacePosition);
                        lengthPiece.X = headPiece.X;
                        lengthPiece.Width = tailPiece.X - headPiece.X;
                    }
                    else
                    {
                        spanPlacementObject.Duration = Math.Abs(endTime - originalStartTime);
                        tailPiece.Position = ToLocalSpace(result.ScreenSpacePosition);
                        lengthPiece.Width = tailPiece.X - headPiece.X;
                    }
                }
            }
            else
            {
                lengthPiece.Position = headPiece.Position = tailPiece.Position = ToLocalSpace(result.ScreenSpacePosition);

                if (result.Time is double startTime)
                    originalStartTime = HitObject.StartTime = startTime;
            }
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

        public override void UpdatePosition(SnapResult result)
        {
            piece.Position = ToLocalSpace(result.ScreenSpacePosition);
            base.UpdatePosition(result);
        }
    }

    public class LengthPiece : CompositeDrawable
    {
        public LengthPiece()
        {
            Origin = Anchor.CentreLeft;

            InternalChild = new Container
            {
                Masking = true,
                Colour = Color4.Yellow,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 8,
                    },
                    new Box
                    {
                        Origin = Anchor.BottomLeft,
                        Anchor = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = 8,
                    }
                }
            };
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
