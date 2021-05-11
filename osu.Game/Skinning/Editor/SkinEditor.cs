// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;

namespace osu.Game.Skinning.Editor
{
    public class SkinEditor : FocusedOverlayContainer
    {
        public const double TRANSITION_DURATION = 500;

        private readonly Drawable target;

        private OsuTextFlowContainer headerText;

        protected override bool StartHidden => true;

        public SkinEditor(Drawable target)
        {
            this.target = target;

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
                    new SkinBlueprintContainer(target),
                    new SkinComponentToolbox(600)
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RequestPlacement = placeComponent
                    }
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
            var instance = (Drawable)Activator.CreateInstance(type);

            var targetContainer = target.ChildrenOfType<IDefaultSkinnableTarget>().FirstOrDefault();

            targetContainer?.Add(instance);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Show();
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
