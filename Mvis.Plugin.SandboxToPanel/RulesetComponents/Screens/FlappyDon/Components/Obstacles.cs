using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Utils;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.FlappyDon.Components
{
    public partial class Obstacles : CompositeDrawable
    {
        /// <summary>
        /// The bounding size of the collision box representing the bird
        /// </summary>
        public Vector2 CollisionBoxSize = new Vector2(50);

        /// <summary>
        /// Whether the obstacles are presently animating or not.
        /// </summary>
        public bool Running { get; private set; }

        /// <summary>
        /// A horizontal X value from the left that indicates where the bird is on the X-axis.
        /// When a pipe crosses over this threshold, it counts as a point to the player.
        /// </summary>
        public float BirdThreshold = 0.0f;

        /// <summary>
        /// An event handler that is called each time a pipe crosses the X
        /// position of the bird, which should increment the score.
        /// </summary>
        public Action<int> ThresholdCrossed;

        /// <summary>
        /// An internal counter for the number of pipes that have crossed the threshold
        /// in order to correctly call the event handler once per threshold.
        /// </summary>
        private int crossedThresholdCount;

        /// <summary>
        /// A collection holding all the obstacles to remove during the update step.
        /// </summary>
        private Stack<Drawable> obstaclesToRemove = new Stack<Drawable>();

        /// <summary>
        /// A counter that keeps track of the number of pipes spawned in order to track
        /// how many have passed the threshold for score tracking.
        /// </summary>
        private int obstacleCount;

        /// <summary>
        /// If the pipes are visible on screen, but their animation has been stopped.
        /// </summary>
        private bool frozen;

        private float pipeVelocity
        {
            get
            {
                if (Clock.FramesPerSecond > 0.0f)
                    return 250 * (float)(Clock.ElapsedFrameTime / 1000.0f);

                return 0.0f;
            }
        }

        private const float pipe_distance = 350.0f;

        public Obstacles()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }

        public void Start()
        {
            if (Running)
                return;

            Running = true;
        }

        public void Freeze()
        {
            if (!Running)
                return;

            frozen = true;
        }

        public void Reset()
        {
            if (!Running) return;

            Running = false;
            frozen = false;

            obstacleCount = 0;
            crossedThresholdCount = 0;

            // Remove all child pipes
            // from this container
            ClearInternal();
        }

        public bool CheckForCollision(Quad birdQuad) => InternalChildren.Cast<PipeObstacle>().FirstOrDefault()?.CheckCollision(birdQuad) ?? false;

        protected override void Update()
        {
            if (!Running) return;

            if (InternalChildren.Count == 0)
            {
                spawnNewObstacle();
                return;
            }

            // Update the position of each pipe obstacle, and de-queue ones that go off screen
            foreach (var drawable in InternalChildren)
            {
                if (frozen)
                    break;

                var obstacle = (PipeObstacle)drawable;
                obstacle.Position = new Vector2(obstacle.Position.X - pipeVelocity, 0.0f);

                if (obstacle.Position.X + obstacle.DrawWidth < 0.0f)
                {
                    // Add the obstacle to the stack to remove later
                    // because InternalChildren can't be updated while being enumerated.
                    obstaclesToRemove.Push(obstacle);
                }
            }

            while (obstaclesToRemove.TryPop(out var obstacle))
            {
                RemoveInternal(obstacle, true);

                // Increase the obstacle count, which will reset threshold detection
                // for the pipe after this one.
                obstacleCount++;
            }

            // When we cross the threshold, increment the score counter, and call the event handler
            var first = InternalChildren.First();

            if (first.X < BirdThreshold && obstacleCount == crossedThresholdCount)
            {
                crossedThresholdCount++;

                // Alert the observer that the threshold was crossed in this update loop
                ThresholdCrossed?.Invoke(crossedThresholdCount);
            }

            // Spawn a new pipe when sufficient distance has passed
            if (InternalChildren.Count > 0)
            {
                var lastObstacle = (PipeObstacle)InternalChildren.Last();
                if (lastObstacle.Position.X + lastObstacle.DrawWidth < DrawWidth - pipe_distance)
                    spawnNewObstacle();
            }
            else
                spawnNewObstacle();
        }

        private void spawnNewObstacle()
        {
            AddInternal(new PipeObstacle
            {
                Position = new Vector2(DrawWidth, 0.0f),
                VerticalPositionAdjust = RNG.NextSingle(-140.0f, 60.0f)
            });
        }
    }
}
