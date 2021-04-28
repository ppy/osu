// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSkinEditor : OsuTestScene
    {
        private HUDOverlay hudOverlay;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create hud", () =>
            {
                hudOverlay = new HUDOverlay(null, null, null, Array.Empty<Mod>());

                // Add any key just to display the key counter visually.
                hudOverlay.KeyCounter.Add(new KeyCounterKeyboard(Key.Space));

                hudOverlay.ComboCounter.Current.Value = 1;
            });

            AddStep("create editor overlay", () => Add(new SkinEditor(hudOverlay)));
        }

        public class SkinEditor : CompositeDrawable
        {
            private readonly Drawable target;

            public SkinEditor(Drawable target)
            {
                this.target = target;

                RelativeSizeAxes = Axes.Both;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                InternalChildren = new[]
                {
                    target,
                    new SkinBlueprintContainer(target),
                };
            }

            public class SkinBlueprintContainer : BlueprintContainer<SkinnableHUDComponent>
            {
                private readonly Drawable target;

                public SkinBlueprintContainer(Drawable target)
                {
                    this.target = target;
                }

                protected override void LoadComplete()
                {
                    base.LoadComplete();

                    SkinnableHUDComponent[] components = target.ChildrenOfType<SkinnableHUDComponent>().ToArray();

                    foreach (var c in components)
                    {
                        Logger.Log($"Adding blueprint for {c.GetType()}");
                        AddBlueprintFor(c);
                    }
                }

                protected override SelectionHandler<SkinnableHUDComponent> CreateSelectionHandler() => new SkinSelectionHandler();

                public class SkinSelectionHandler : SelectionHandler<SkinnableHUDComponent>
                {
                    protected override void DeleteItems(IEnumerable<SkinnableHUDComponent> items)
                    {
                        foreach (var i in items)
                            i.Drawable.Expire();
                    }

                    protected override void OnSelectionChanged()
                    {
                        base.OnSelectionChanged();

                        SelectionBox.CanRotate = true;
                        SelectionBox.CanScaleX = true;
                        SelectionBox.CanScaleY = true;
                        SelectionBox.CanReverse = false;
                    }

                    public override bool HandleRotation(float angle)
                    {
                        foreach (var c in SelectedBlueprints)
                            c.Item.SkinRotation.Value += angle;

                        return base.HandleRotation(angle);
                    }

                    public override bool HandleScale(Vector2 scale, Anchor anchor)
                    {
                        adjustScaleFromAnchor(ref scale, anchor);

                        foreach (var c in SelectedBlueprints)
                            c.Item.SkinScale.Value += scale.X * 0.01f;

                        return true;
                    }

                    public override bool HandleMovement(MoveSelectionEvent<SkinnableHUDComponent> moveEvent)
                    {
                        foreach (var c in SelectedBlueprints)
                            c.Item.SkinPosition.Value += moveEvent.InstantDelta.X;
                        return true;
                    }

                    private static void adjustScaleFromAnchor(ref Vector2 scale, Anchor reference)
                    {
                        // cancel out scale in axes we don't care about (based on which drag handle was used).
                        if ((reference & Anchor.x1) > 0) scale.X = 0;
                        if ((reference & Anchor.y1) > 0) scale.Y = 0;

                        // reverse the scale direction if dragging from top or left.
                        if ((reference & Anchor.x0) > 0) scale.X = -scale.X;
                        if ((reference & Anchor.y0) > 0) scale.Y = -scale.Y;
                    }
                }

                protected override SelectionBlueprint<SkinnableHUDComponent> CreateBlueprintFor(SkinnableHUDComponent component)
                    => new SkinBlueprint(component);

                public class SkinBlueprint : SelectionBlueprint<SkinnableHUDComponent>
                {
                    /// <summary>
                    /// The <see cref="DrawableHitObject"/> which this <see cref="OverlaySelectionBlueprint"/> applies to.
                    /// </summary>
                    public readonly SkinnableHUDComponent Component;

                    private Container box;
                    private Drawable drawable => Component.Drawable;

                    /// <summary>
                    /// Whether the blueprint should be shown even when the <see cref="Component"/> is not alive.
                    /// </summary>
                    protected virtual bool AlwaysShowWhenSelected => false;

                    protected override bool ShouldBeAlive => (Component.IsAlive && Component.IsPresent) || (AlwaysShowWhenSelected && State == SelectionState.Selected);

                    public SkinBlueprint(SkinnableHUDComponent component)
                        : base(component)
                    {
                        Component = component;
                    }

                    [BackgroundDependencyLoader]
                    private void load(OsuColour colours)
                    {
                        InternalChildren = new Drawable[]
                        {
                            box = new Container
                            {
                                Colour = colours.Yellow,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.2f,
                                        AlwaysPresent = true,
                                    },
                                }
                            },
                        };
                    }

                    private Quad drawableQuad;

                    public override Quad ScreenSpaceDrawQuad => drawableQuad;

                    protected override void Update()
                    {
                        base.Update();

                        drawableQuad = drawable.ScreenSpaceDrawQuad;
                        var quad = ToLocalSpace(drawable.ScreenSpaceDrawQuad);

                        box.Position = quad.TopLeft;
                        box.Size = quad.Size;
                        box.Rotation = Component.Rotation;
                    }

                    public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => drawable.ReceivePositionalInputAt(screenSpacePos);

                    public override Vector2 ScreenSpaceSelectionPoint => Component.ToScreenSpace(Vector2.Zero);

                    public override Quad SelectionQuad => drawable.ScreenSpaceDrawQuad;

                    public override Vector2 GetInstantDelta(Vector2 screenSpacePosition) => Component.Parent.ToLocalSpace(screenSpacePosition) - Component.Position;
                }
            }
        }
    }
}
