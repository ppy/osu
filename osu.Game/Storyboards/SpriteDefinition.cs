// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Game.Storyboards.Drawables;
using System.Collections.Generic;

namespace osu.Game.Storyboards
{
    public class SpriteDefinition : CommandTimelineGroup, ElementDefinition
    {
        public string Path { get; private set; }
        public Anchor Origin;
        public Vector2 InitialPosition;

        private List<CommandLoop> loops = new List<CommandLoop>();
        private List<CommandTrigger> triggers = new List<CommandTrigger>();
        
        public SpriteDefinition(string path, Anchor origin, Vector2 initialPosition)
        {
            Path = path;
            Origin = origin;
            InitialPosition = initialPosition;
        }

        public CommandLoop AddLoop(double startTime, int loopCount)
        {
            var loop = new CommandLoop(startTime, loopCount);
            loops.Add(loop);
            return loop;
        }

        public CommandTrigger AddTrigger(string triggerName, double startTime, double endTime, int groupNumber)
        {
            var trigger = new CommandTrigger(triggerName, startTime, endTime, groupNumber);
            triggers.Add(trigger);
            return trigger;
        }

        public virtual Drawable CreateDrawable()
            => new StoryboardSprite(this);

        public override void ApplyTransforms(Drawable target)
        {
            base.ApplyTransforms(target);
            foreach (var loop in loops)
                loop.ApplyTransforms(target);

            // TODO
            return;
            foreach (var trigger in triggers)
                trigger.ApplyTransforms(target);
        }

        public override string ToString()
            => $"{Path}, {Origin}, {InitialPosition}";
    }
}
