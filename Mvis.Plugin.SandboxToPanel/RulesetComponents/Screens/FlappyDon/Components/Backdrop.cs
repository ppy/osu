using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.FlappyDon.Components
{
    public partial class Backdrop : Container<Sprite>
    {
        public bool Running { get; private set; }

        private Vector2 lastSize;
        private readonly Func<Sprite> createSprite;
        private readonly double duration;

        public Backdrop(Func<Sprite> createSprite, double duration = 2000.0f)
        {
            this.createSprite = createSprite;
            this.duration = duration;

            RelativeSizeAxes = Axes.Both;
            Add(createSprite());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateLayout();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!DrawSize.Equals(lastSize))
            {
                updateLayout();
                lastSize = DrawSize;
            }
        }

        /// <summary>
        /// Start animating the child sprites across the screen
        /// </summary>
        public void Start()
        {
            if (Running)
                return;

            Running = true;

            if (!IsLoaded)
                return;

            updateLayout();
        }

        /// <summary>
        /// Cancel the animations, but leave all sprites in place.
        /// </summary>
        public void Freeze()
        {
            if (!Running || !IsLoaded)
                return;

            Running = false;

            if (!IsLoaded)
                return;

            stopAnimatingChildren();
        }

        private void updateLayout()
        {
            // Get an initial sprite we can use to derive size
            var sprite = Children[0];

            // Work out how many copies are needed to horizontally fill the screen
            var spriteNum = (int)Math.Ceiling(DrawWidth / sprite.DrawWidth) + 1;

            // If the number needed is higher or lower than the current number of child sprites, add/remove the amount needed for them to match
            if (spriteNum != Children.Count)
            {
                // Update the number of sprites in the list to match the number we need to cover the whole container
                while (Children.Count > spriteNum)
                    Remove(Children[Children.Count - 1], true);

                while (Children.Count < spriteNum)
                    Add(createSprite());
            }

            // Lay out all of the child sprites horizontally, and assign a looping pan animation to create the effect of constant scrolling
            var offset = 0f;

            foreach (var childSprite in Children)
            {
                var width = childSprite.DrawWidth;
                childSprite.X = offset;

                if (Running)
                    childSprite.Loop(b => b.MoveToX(offset).Then().MoveToX(offset - width, duration));

                offset += width;
            }
        }

        private void stopAnimatingChildren() => ClearTransforms(true);
    }
}
