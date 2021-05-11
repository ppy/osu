// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    public class SkinEditor : FocusedOverlayContainer
    {
        public const double TRANSITION_DURATION = 500;

        private readonly Drawable targetScreen;

        private OsuTextFlowContainer headerText;

        protected override bool StartHidden => true;

        [Resolved]
        private SkinManager skins { get; set; }

        private Bindable<Skin> currentSkin;

        public SkinEditor(Drawable targetScreen)
        {
            this.targetScreen = targetScreen;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    headerText = new OsuTextFlowContainer
                    {
                        TextAnchor = Anchor.TopCentre,
                        Padding = new MarginPadding(20),
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X
                    },
                    new SkinBlueprintContainer(targetScreen),
                    new SkinComponentToolbox(600)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RequestPlacement = placeComponent
                    },
                    new FillFlowContainer
                    {
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Spacing = new Vector2(5),
                        Padding = new MarginPadding
                        {
                            Top = 10,
                            Left = 10,
                        },
                        Margin = new MarginPadding
                        {
                            Right = 10,
                            Bottom = 10,
                        },
                        Children = new Drawable[]
                        {
                            new TriangleButton
                            {
                                Text = "Save Changes",
                                Width = 140,
                                Action = save,
                            },
                            new DangerousTriangleButton
                            {
                                Text = "Revert to default",
                                Width = 140,
                                Action = revert,
                            },
                        }
                    },
                }
            };

            headerText.AddParagraph("Skin editor (preview)", cp => cp.Font = OsuFont.Default.With(size: 24));
            headerText.AddParagraph("This is a preview of what is to come. Changes are lost on changing screens.", cp =>
            {
                cp.Font = OsuFont.Default.With(size: 12);
                cp.Colour = colours.Yellow;
            });
        }

        private void placeComponent(Type type)
        {
            Drawable instance = (Drawable)Activator.CreateInstance(type);

            getTarget(SkinnableTarget.MainHUDComponents)?.Add(instance);
        }

        private ISkinnableTarget getTarget(SkinnableTarget target)
        {
            return targetScreen.ChildrenOfType<ISkinnableTarget>().FirstOrDefault(c => c.Target == target);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Show();

            // as long as the skin editor is loaded, let's make sure we can modify the current skin.
            currentSkin = skins.CurrentSkin.GetBoundCopy();

            // schedule ensures this only happens when the skin editor is visible.
            // also avoid some weird endless recursion / bindable feedback loop (something to do with tracking skins across three different bindable types).
            // probably something which will be factored out in a future database refactor so not too concerning for now.
            currentSkin.BindValueChanged(skin => Scheduler.AddOnce(skins.EnsureMutableSkin), true);
        }

        private void revert()
        {
            SkinnableElementTargetContainer[] targetContainers = targetScreen.ChildrenOfType<SkinnableElementTargetContainer>().ToArray();

            foreach (var t in targetContainers)
            {
                currentSkin.Value.ResetDrawableTarget(t);

                // add back default components
                getTarget(t.Target).Reload();
            }
        }

        private void save()
        {
            SkinnableElementTargetContainer[] targetContainers = targetScreen.ChildrenOfType<SkinnableElementTargetContainer>().ToArray();

            foreach (var t in targetContainers)
                currentSkin.Value.UpdateDrawableTarget(t);

            skins.Save(skins.CurrentSkin.Value);
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override void PopIn()
        {
            this.FadeIn(TRANSITION_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
