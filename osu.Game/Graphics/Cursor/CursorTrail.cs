//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.MathUtils;
using OpenTK;

namespace osu.Game.Graphics.Cursor
{
    class TrailSprite : Sprite
    {
        private Shader shader;

        public TrailSprite()
        {
            Origin = Anchor.Centre;
        }

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            SpriteDrawNode sNode = node as SpriteDrawNode;
            sNode.RoundedTextureShader = sNode.TextureShader = shader;
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders, TextureStore textures)
        {
            shader = shaders?.Load(@"CursorTrail", FragmentShaderDescriptor.Texture);
            Texture = textures.Get(@"Cursor/cursortrail");
        }
    }

    class CursorTrail : Container<TrailSprite>
    {
        public override bool Contains(Vector2 screenSpacePos) => true;

        int currentIndex;

        protected override bool CanBeFlattened => false;

        Shader shader;

        private double timeOffset;

        private float time;

        const int MAX_SPRITES = 2048;

        Vector2? lastPosition;

        protected override DrawNode CreateDrawNode() => new TrailDrawNode();

        protected override void ApplyDrawNode(DrawNode node)
        {
            base.ApplyDrawNode(node);

            TrailDrawNode tNode = node as TrailDrawNode;
            tNode.Shader = shader;
            tNode.Time = time;
        }

        public CursorTrail()
        {
            RelativeSizeAxes = Axes.Both;

            for (int i = 0; i < MAX_SPRITES; i++)
                AddInternal(new TrailSprite());
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            shader = shaders?.Load(@"CursorTrail", FragmentShaderDescriptor.Texture);
        }

        protected override void Update()
        {
            base.Update();

            Invalidate(Invalidation.DrawNode);

            int fadeClockResetThreshold = 1000000;

            time = (float)(Time.Current - timeOffset) / 500f;
            if (time > fadeClockResetThreshold)
                ResetTime();
        }

        private void ResetTime()
        {
            foreach (var c in Children)
                c.Alpha -= time;

            time = 0;
            timeOffset = Time.Current;
        }

        protected override bool OnMouseMove(InputState state)
        {
            if (lastPosition == null)
            {
                lastPosition = state.Mouse.Position;
                return base.OnMouseMove(state);
            }

            Vector2 pos1 = lastPosition.Value;
            Vector2 pos2 = state.Mouse.Position;

            Vector2 diff = pos2 - pos1;
            float distance = diff.Length;
            Vector2 direction = diff / distance;

            float interval = this[0].DrawSize.X / 2;

            for (float d = interval; d < distance; d += interval)
            {
                lastPosition = pos1 + direction * d;
                addPosition(lastPosition.Value);
            }

            return base.OnMouseMove(state);
        }

        private void addPosition(Vector2 pos)
        {
            var s = this[currentIndex];
            s.Position = pos;
            s.Alpha = time + 1f;

            currentIndex = (currentIndex + 1) % MAX_SPRITES;
        }

        class TrailDrawNode : ContainerDrawNode
        {
            public new Shader Shader;
            public float Time;

            public override void Draw(IVertexBatch vertexBatch)
            {
                Shader.GetUniform<float>("g_FadeClock").Value = Time;
                base.Draw(vertexBatch);
            }
        }
    }
}