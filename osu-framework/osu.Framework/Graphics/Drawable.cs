//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using OpenTK;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Batches;
using osu.Framework.Graphics.OpenGL;
using osu.Framework.Timing;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Lists;
using osu.Framework.Cached;
using osu.Framework.MathUtils;
using System.Threading;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;

namespace osu.Framework.Graphics
{
    public partial class Drawable : IDisposable, IHasLifetime
    {
        public event VoidDelegate OnUpdate;

        private LifetimeList<Drawable> internalChildren;
        public ReadOnlyList<Drawable> Children
        {
            get
            {
                ensureMainThread();
                return internalChildren;
            }
        }

        protected virtual IVertexBatch ActiveBatch => Parent?.ActiveBatch;

        private List<Transformation> transformations = new List<Transformation>();
        public List<Transformation> Transformations
        {
            get
            {
                ensureMainThread();
                return transformations;
            }
        }

        private Vector2 position;
        public Vector2 Position
        {
            get { return position; }
            set
            {
                if (position == value) return;
                position = value;

                Invalidate(false);
            }
        }

        private Vector2 customOrigin;
        public virtual Vector2 OriginPosition
        {
            get
            {
                if (Origin == Anchor.Custom)
                    return customOrigin;

                if (!HasDefinedSize) return Vector2.Zero;

                Vector2 origin = Vector2.Zero;

                if ((Origin & Anchor.x1) > 0)
                    origin.X += ActualSize.X / 2f;
                else if ((Origin & Anchor.x2) > 0)
                    origin.X += ActualSize.X;

                if ((Origin & Anchor.y1) > 0)
                    origin.Y += ActualSize.Y / 2f;
                else if ((Origin & Anchor.y2) > 0)
                    origin.Y += ActualSize.Y;

                return origin;
            }

            set
            {
                customOrigin = value;
                Origin = Anchor.Custom;
            }
        }

        private Vector2 vectorScale = Vector2.One;
        public Vector2 VectorScale
        {
            get
            {
                return vectorScale;
            }

            set
            {
                if (vectorScale == value) return;
                vectorScale = value;

                Invalidate();
            }
        }


        private Color4 colour = Color4.White;
        public Color4 Colour
        {
            get { return colour; }

            set
            {
                if (colour == value) return;
                colour = value;

                Invalidate(false, false);
            }
        }

        private Anchor anchor;
        public Anchor Anchor
        {
            get { return anchor; }

            set
            {
                if (anchor == value) return;
                anchor = value;

                Invalidate();
            }
        }

        public virtual string ToolTip { get; set; }

        private float rotation;
        public float Rotation
        {
            get { return rotation; }

            set
            {
                if (value == rotation) return;
                rotation = value;

                Invalidate();
            }
        }

        private float scale = 1.0f;
        public float Scale
        {
            get { return scale; }

            set
            {
                if (value == scale)
                    return;
                scale = value;

                Invalidate();
            }
        }

        private float alpha = 1.0f;
        public float Alpha
        {
            get { return alpha; }

            set
            {
                if (alpha == value) return;

                Invalidate(alpha == 0 || value == 0, alpha == 0 || value == 0);

                alpha = value;
            }
        }

        public bool IsDisposable;

        private Vector2 size = Vector2.One;
        public virtual Vector2 Size
        {
            get { return size; }
            set
            {
                if (size == value)
                    return;
                size = value;

                Invalidate();
            }
        }

        private InheritMode sizeMode;
        public InheritMode SizeMode
        {
            get { return sizeMode; }
            set
            {
                if (value == sizeMode)
                    return;
                sizeMode = value;

                Invalidate();
            }
        }

        private InheritMode positionMode;
        public InheritMode PositionMode
        {
            get { return positionMode; }
            set
            {
                if (value == positionMode)
                    return;
                positionMode = value;

                Invalidate();
            }
        }

        /// <summary>
        /// The real pixel size of this drawable.
        /// </summary>
        public virtual Vector2 ActualSize
        {

            get
            {
                Vector2 size = Size;
                if ((SizeMode & InheritMode.X) > 0)
                    size.X *= Parent?.InheritableWidth ?? 1;
                if ((SizeMode & InheritMode.Y) > 0)
                    size.Y *= Parent?.InheritableHeight ?? 1;

                return size;
            }
        }

        /// <summary>
        /// The real pixel position of this drawable.
        /// </summary>
        public Vector2 ActualPosition
        {
            get
            {
                Vector2 pos = Position;
                if ((PositionMode & InheritMode.X) > 0)
                    pos.X *= Parent?.InheritableWidth ?? 1;
                if ((PositionMode & InheritMode.Y) > 0)
                    pos.Y *= Parent?.InheritableHeight ?? 1;

                return pos;
            }
        }

        public virtual Quad ScreenSpaceInputQuad => ScreenSpaceDrawQuad;
        private Cached<Quad> screenSpaceDrawQuadBacking = new Cached<Quad>();
        public Quad ScreenSpaceDrawQuad => screenSpaceDrawQuadBacking.Refresh(delegate
        {
            Quad result = GetScreenSpaceQuad(DrawQuad);

            //if (PixelSnapping ?? CheckForcedPixelSnapping(result))
            //{
            //    Vector2 adjust = new Vector2(
            //        (float)Math.Round(result.TopLeft.X) - result.TopLeft.X,
            //        (float)Math.Round(result.TopLeft.Y) - result.TopLeft.Y
            //        );

            //    result.TopLeft += adjust;
            //    result.TopRight += adjust;
            //    result.BottomLeft += adjust;
            //    result.BottomRight += adjust;
            //}

            return result;
        });

        private Anchor origin;
        public virtual Anchor Origin
        {
            get
            {
                Anchor origin = this.origin;
                if (flipHorizontal)
                {
                    if ((origin & Anchor.x0) > 0)
                        origin = (origin & ~Anchor.x0) | Anchor.x2;
                    else if ((origin & Anchor.x2) > 0)
                        origin = (origin & ~Anchor.x2) | Anchor.x0;
                }
                if (flipVertical)
                {
                    if ((origin & Anchor.y0) > 0)
                        origin = (origin & ~Anchor.y0) | Anchor.y2;
                    else if ((origin & Anchor.y2) > 0)
                        origin = (origin & ~Anchor.y2) | Anchor.y0;
                }
                return origin;
            }
            set
            {
                if (origin == value)
                    return;
                origin = value;
                Invalidate();
            }
        }

        private float depth;
        public float Depth
        {
            get { return depth; }
            set
            {
                if (depth == value)
                    return;
                depth = value;

                Parent?.depthChangeQueue.Enqueue(this);
            }
        }

        protected virtual bool HasDefinedSize => true;

        public float Width
        {
            get { return Size.X; }
            set { Size = new Vector2(value, Size.Y); }
        }
        public float Height
        {
            get { return Size.Y; }
            set { Size = new Vector2(Size.X, value); }
        }

        internal virtual float InheritableWidth => ActualSize.X;
        internal virtual float InheritableHeight => ActualSize.Y;

        protected virtual IFrameBasedClock Clock => clockBacking.Refresh(() => Parent?.Clock);
        private Cached<IFrameBasedClock> clockBacking = new Cached<IFrameBasedClock>();

        protected double Time => Clock?.CurrentTime ?? 0;

        private bool flipVertical;
        public bool FlipVertical
        {
            get { return flipVertical; }
            set
            {
                if (FlipVertical == value)
                    return;
                flipVertical = value;
                Invalidate();
            }
        }

        private bool flipHorizontal;
        public bool FlipHorizontal
        {
            get { return flipHorizontal; }
            set
            {
                if (FlipHorizontal == value)
                    return;
                flipHorizontal = value;
                Invalidate();
            }
        }

        public virtual bool IsVisible => Alpha > 0.0001f && IsAlive && Parent?.IsVisible == true;

        private BlendingFactorSrc blendingSrc = BlendingFactorSrc.SrcAlpha;
        private BlendingFactorDest blendingDst = BlendingFactorDest.OneMinusSrcAlpha;

        public virtual bool Additive
        {
            get { return blendingDst == BlendingFactorDest.One && blendingSrc == BlendingFactorSrc.SrcAlpha; }
            set { blendingDst = value ? BlendingFactorDest.One : BlendingFactorDest.OneMinusSrcAlpha; }
        }

        protected virtual bool? PixelSnapping { get; set; }

        private Cached<DrawInfo> drawInfoBacking = new Cached<DrawInfo>();

        protected DrawInfo DrawInfo => drawInfoBacking.Refresh(delegate
        {
            DrawInfo di = BaseDrawInfo;

            Vector2 scale = VectorScale * Scale;

            float alpha = Alpha;
            if (Colour.A > 0 && Colour.A < 1)
                alpha *= Colour.A;

            Color4 colour = new Color4(Colour.R, Colour.G, Colour.B, alpha);

            if (Parent == null)
                di.ApplyTransform(di, GetAnchoredPosition(ActualPosition), scale, Rotation, OriginPosition, colour);
            else
                Parent.DrawInfo.ApplyTransform(di, GetAnchoredPosition(ActualPosition), scale, Rotation, OriginPosition, colour);

            return di;
        });

        protected virtual DrawInfo BaseDrawInfo => new DrawInfo();

        protected virtual Quad DrawQuad
        {
            get
            {
                if (!HasDefinedSize)
                    return new Quad();

                Vector2 s = ActualSize;

                //most common use case gets a shortcut
                if (!flipHorizontal && !flipVertical) return new Quad(0, 0, s.X, s.Y);

                if (flipHorizontal && flipVertical)
                    return new Quad(s.X, s.Y, -s.X, -s.Y);
                if (flipHorizontal)
                    return new Quad(s.X, 0, -s.X, s.Y);
                return new Quad(0, s.Y, s.X, -s.Y);
            }
        }

        /// <summary>
        /// A queue of children to have their depths re-sorted after their Drawable.Depth is modified.
        /// </summary>
        private Queue<Drawable> depthChangeQueue = new Queue<Drawable>();

        public Drawable Parent { get; private set; }

        protected virtual IComparer<Drawable> DepthComparer => new DepthComparer();

        public Drawable()
        {
            internalChildren = new LifetimeList<Drawable>(DepthComparer);
        }

        /// <summary>
        /// Checks if this drawable is a child of parent regardless of nesting depth.
        /// </summary>
        /// <param name="parent">The parent to search for.</param>
        /// <returns>If this drawable is a child of parent.</returns>
        public bool IsChildOfRecursive(Drawable parent)
        {
            if (parent == null)
                return false;

            // Do a bottom-up recursion for efficiency
            Drawable currentParent = Parent;
            while (currentParent != null)
            {
                if (currentParent == parent)
                    return true;
                currentParent = currentParent.Parent;
            }

            return false;
        }

        /// <summary>
        /// Checks if this drawable is a parent of child regardless of nesting depth.
        /// </summary>
        /// <param name="child">The child to search for.</param>
        /// <returns>If this drawable is a parent of child.</returns>
        public bool IsParentOfRecursive(Drawable child)
        {
            if (child == null)
                return false;

            // Do a bottom-up recursion for efficiency
            Drawable currentParent = child.Parent;
            while (currentParent != null)
            {
                if (currentParent == this)
                    return true;
                currentParent = currentParent.Parent;
            }

            return false;
        }

        protected virtual Drawable Add(Drawable drawable)
        {
            if (drawable == null)
                return null;

            drawable.changeParent(this);

            internalChildren.Add(drawable);

            Invalidate();
            return drawable;
        }

        protected void Add(IEnumerable<Drawable> collection)
        {
            foreach (Drawable d in collection)
                Add(d);
        }

        protected bool Remove(Drawable p, bool dispose = true)
        {
            if (p == null)
                return false;

            bool result = internalChildren.Remove(p);

            Invalidate();
            p.Parent = null;

            if (dispose && p.IsDisposable)
                p.Dispose();
            else
                p.Invalidate();

            return result;
        }

        protected int RemoveAll(Predicate<Drawable> match, bool dispose = true)
        {
            List<Drawable> toRemove = internalChildren.FindAll(match);
            for (int i = 0; i < toRemove.Count; i++)
                Remove(toRemove[i]);

            return toRemove.Count;
        }

        protected void Remove(IEnumerable<Drawable> range, bool dispose = true)
        {
            if (range == null)
                return;

            foreach (Drawable p in range)
            {
                if (p.IsDisposable)
                    p.Dispose();
                Remove(p);
            }
        }

        protected void Clear(bool dispose = true)
        {
            foreach (Drawable t in Children)
            {
                if (dispose)
                    t.Dispose();
                t.Parent = null;
            }

            internalChildren.Clear();

            Invalidate();
        }

        protected void DrawSubTree()
        {
            if (!IsVisible)
                return;

            // Pre-sort, covers the root drawable
            updateDepthChanges();

            PreDraw();

            GLWrapper.SetBlend(blendingSrc, blendingDst);

            // Draw this
            Draw();

            // Post-sort, in-case Draw() changed the child depths
            updateDepthChanges();

            // Draw children
            foreach (Drawable child in internalChildren.Current)
                child.DrawSubTree();

            PostDraw();

            ActiveBatch?.Draw();
        }

        /// <summary>
        /// Executes before this drawable is drawn.
        /// </summary>
        protected virtual void PreDraw()
        {
        }

        /// <summary>
        /// Executes after this drawable and all of its children are drawn.
        /// </summary>
        protected virtual void PostDraw()
        {
        }

        protected virtual Quad DrawQuadForBounds => DrawQuad;

        private delegate Vector2 BoundsResult();
        private Cached<Vector2> boundingSizeBacking = new Cached<Vector2>();
        internal Vector2 GetBoundingSize(Drawable calculateDrawable)
        {
            BoundsResult computeBoundingSize = () =>
            {
                Quad drawQuad = DrawQuadForBounds;

                if (calculateDrawable == this)
                    return drawQuad.BottomRight;

                //field will be none when the drawable isn't requesting auto-sizing
                Quad q = calculateDrawable.DrawInfo.MatrixInverse * GetScreenSpaceQuad(drawQuad);
                Vector2 a = Parent == null ? Vector2.Zero : (GetAnchoredPosition(Vector2.Zero) * Parent.DrawInfo.Matrix) * calculateDrawable.DrawInfo.MatrixInverse;

                Vector2 bounds = new Vector2(0, 0);

                // Without this, 0x0 objects (like FontText with no string) produce weird results.
                // When all vertices of the quad are at the same location, then the object is effectively invisible.
                // Thus we don't need its actual bounding box, but can just assume a size of 0.
                if (q.TopLeft == q.TopRight && q.TopLeft == q.BottomLeft && q.TopLeft == q.BottomRight)
                    return bounds;

                foreach (Vector2 p in new[] { q.TopLeft, q.TopRight, q.BottomLeft, q.BottomRight })
                {
                    // Compute the clipped offset depending on anchoring.
                    Vector2 offset;

                    if (Anchor == Anchor.CentreRight || Anchor == Anchor.TopRight || Anchor == Anchor.BottomRight)
                        offset.X = a.X - p.X;
                    else if (Anchor == Anchor.CentreLeft || Anchor == Anchor.TopLeft || Anchor == Anchor.BottomLeft)
                        offset.X = p.X - a.X;
                    else
                        offset.X = Math.Abs(p.X - a.X);

                    if (Anchor == Anchor.BottomCentre || Anchor == Anchor.BottomLeft || Anchor == Anchor.BottomRight)
                        offset.Y = a.Y - p.Y;
                    else if (Anchor == Anchor.TopCentre || Anchor == Anchor.TopLeft || Anchor == Anchor.TopRight)
                        offset.Y = p.Y - a.Y;
                    else
                        offset.Y = Math.Abs(p.Y - a.Y);

                    // Expand bounds according to clipped offset
                    bounds.X = Math.Max(bounds.X, offset.X);
                    bounds.Y = Math.Max(bounds.Y, offset.Y);
                }

                // When anchoring an object at the center of the parent, then the parent's size needs to be twice as big
                // as the child's size.
                switch (Anchor)
                {
                    case Anchor.TopCentre:
                    case Anchor.Centre:
                    case Anchor.BottomCentre:
                        bounds.X *= 2;
                        break;
                }

                switch (Anchor)
                {
                    case Anchor.CentreLeft:
                    case Anchor.Centre:
                    case Anchor.CentreRight:
                        bounds.Y *= 2;
                        break;
                }

                return bounds;
            };

            Debug.Assert(calculateDrawable == this || (Parent != null && calculateDrawable == Parent), "We only ever request the bounding size in either our own coordinate frame, or in the parent's.");

            // There are exactly two types of bounding sizes ever requested:
            //  1. The bounding size of oneself in order to update ones own autosize (called in UpdateSubTree)
            //     This case is handled by the first branch of the following if statement. It doesn't need to be cached
            //     since it's invoked at most once per frame.
            //  2. The bounding size with respect to the first parent object up the hierarchy with an autosize setting.
            //     This case is handled by the second branch of the following if statement. Caching it significantly
            //     Increases performance, since it may be queried up to n times per frame where n is the depth of the
            //     hierarchy.

            if (calculateDrawable == this)
                return computeBoundingSize();
            return boundingSizeBacking.Refresh(() => computeBoundingSize());
        }

        protected void UpdateDrawInfoSubtree()
        {
            if (drawInfoBacking.IsValid)
            {
                DrawInfo oldDrawInfo = DrawInfo;

                drawInfoBacking.Invalidate();

                if (oldDrawInfo.Equals(DrawInfo))
                    return;
            }

            foreach (Drawable child in internalChildren.Current)
                child.UpdateDrawInfoSubtree();
        }

        internal virtual void UpdateSubTree()
        {
            transformationDelay = 0;

            //todo: this should be moved to after the IsVisible condition once we have TOL for transformations (and some better logic).
            updateTransformations();

            if (!IsVisible)
                return;

            Update();
            OnUpdate?.Invoke();

            // This check is conservative in the sense, that autosize containers do not impose
            // any masking on children. This is valid under the assumption, that autosize
            // will always adjust its size such that it does not mask children away.
            // todo: Fix for AlwaysDraw == false (never get to UpdateResult.Discard case below)
            //if (IsMaskedOut())
            //    return updateResult = UpdateResult.ShouldNotDraw;

            updateDepthChanges();

            internalChildren.Update(Time);

            foreach (Drawable child in internalChildren.Current)
                child.UpdateSubTree();

            UpdateLayout();
        }

        /// <summary>
        /// Perform any layout changes just before autosize is calculated.
        /// </summary>
        protected virtual void UpdateLayout()
        {

        }

        protected virtual void Draw()
        {
        }

        protected virtual void Update()
        {
        }

        protected virtual Quad GetScreenSpaceQuad(Quad input)
        {
            if (DrawInfo == null)
                return new Quad(0, 0, 0, 0);

            return DrawInfo.Matrix * input;
        }

        public Quad GetSpaceQuadIn(Drawable parent)
        {
            return parent.DrawInfo.MatrixInverse * ScreenSpaceDrawQuad;
        }

        protected virtual bool CheckForcedPixelSnapping(Quad screenSpaceQuad)
        {
            return false;
        }

        private void changeParent(Drawable parent)
        {
            if (Parent == parent)
                return;

            Parent?.Remove(this, false);
            Parent = parent;

            changeRoot(Parent?.Game);
        }

        private void changeRoot(Game root)
        {
            if (root == null) return;

            Game = root;
            clockBacking.Invalidate();

            Children.ForEach(c => c.changeRoot(root));
        }

        /// <summary>
        /// The time at which this drawable becomes valid (and is considered for drawing).
        /// </summary>
        public double LifetimeStart { get; set; } = double.MinValue;

        /// <summary>
        /// The time at which this drawable is no longer valid (and is considered for disposal).
        /// </summary>
        public double LifetimeEnd { get; set; } = double.MaxValue;

        /// <summary>
        /// Whether this drawable is alive.
        /// </summary>
        public bool IsAlive
        {
            get
            {
                if (LifetimeStart == double.MinValue && LifetimeEnd == double.MaxValue)
                    return true;

                double t = Time;
                return t >= LifetimeStart && t < LifetimeEnd;
            }
        }

        /// <summary>
        /// Whether to remove the drawable from its parent's children when it's not alive.
        /// </summary>
        public virtual bool RemoveWhenNotAlive => true;

        /// <summary>
        /// Override to add delayed load abilities (ie. using IsAlive)
        /// </summary>
        public virtual bool LoadRequired => !loaded;
        private bool loaded;

        public virtual void Load()
        {
            loaded = true;
            Invalidate();
        }

        /// <summary>
        /// Process updates to this drawable based on loaded transformations.
        /// </summary>
        /// <returns>Whether we should draw this drawable.</returns>
        private void updateTransformations()
        {
            List<Transformation> transformations = Transformations;

            int transformationCount = transformations.Count;

            if (transformationCount == 0)
                return;

            bool shouldDraw = true;

            double time = Time;

            bool hasFuture = false;
            bool hasPast = false;

            bool? horizontalFlip = null;
            bool? verticalFlip = null;
            bool? additive = null;

            bool hasRotation = false;
            bool hasScale = false;
            bool hasFade = false;
            bool hasMovement = false, hasMovementX = false, hasMovementY = false;
            bool hasColour = false;
            bool hasUpdateableLoops = false;

            // First check for active transformations
            foreach (Transformation t in transformations)
            {
                if (t.Time1 >= time || t.Time2 > time)
                {
                    hasFuture = true;

                    if (t.Loop && t.CurrentLoopCount > 0)
                        hasUpdateableLoops = true;
                    if (t.Time1 > time)
                        continue;
                }

                if (t.Time2 <= time)
                {
                    hasPast = true;

                    if (t.Loop && (t.MaxLoopCount == 0 || t.CurrentLoopCount < t.MaxLoopCount - 1))
                        hasUpdateableLoops = true;
                    if (t.Time2 < time)
                        continue;
                }

                shouldDraw = true;

                switch (t.Type)
                {
                    case TransformationType.Fade:
                        Alpha = Interpolation.ValueAt(time, t.StartFloat, t.EndFloat, t.Time1, t.Time2, t.Easing);
                        hasFade = true;
                        break;
                    case TransformationType.Movement:
                        Position = Interpolation.ValueAt(time, t.StartVector, t.EndVector, t.Time1, t.Time2, t.Easing);
                        hasMovement = true;
                        break;
                    case TransformationType.MovementX:
                        Position = new Vector2(Interpolation.ValueAt(time, t.StartFloat, t.EndFloat, t.Time1, t.Time2, t.Easing), Position.Y);
                        hasMovementX = true;
                        break;
                    case TransformationType.MovementY:
                        Position = new Vector2(Position.X, Interpolation.ValueAt(time, t.StartFloat, t.EndFloat, t.Time1, t.Time2, t.Easing));
                        hasMovementY = true;
                        break;
                    case TransformationType.Scale:
                        Scale = Interpolation.ValueAt(time, t.StartFloat, t.EndFloat, t.Time1, t.Time2, t.Easing);
                        hasScale = true;
                        break;
                    case TransformationType.VectorScale:
                        VectorScale = Interpolation.ValueAt(time, t.StartVector, t.EndVector, t.Time1, t.Time2, t.Easing);
                        hasScale = true;
                        break;
                    case TransformationType.Rotation:
                        Rotation = Interpolation.ValueAt(time, t.StartFloat, t.EndFloat, t.Time1, t.Time2, t.Easing);
                        hasRotation = true;
                        break;
                    case TransformationType.Colour:
                        Colour = Interpolation.ValueAt(time, t.StartColour, t.EndColour, t.Time1, t.Time2, t.Easing);
                        hasColour = true;
                        break;
                    case TransformationType.ParameterFlipHorizontal:
                        horizontalFlip = FlipHorizontal = true;
                        break;
                    case TransformationType.ParameterFlipVertical:
                        verticalFlip = FlipVertical = true;
                        break;
                    case TransformationType.ParameterAdditive:
                        additive = Additive = true;
                        break;
                }
            }

            // Update loopable transformations before going farther, as transformations may need to be updated again if any is moved.
            if (hasUpdateableLoops)
            {
                // Don't update endless loops if they would be the only thing keeping the sprite visible.
                bool updateEndlessLoops = IsAlive || hasFuture;

                bool transformationsChanged = false;
                bool keepSorted = true;

                // Iterate backwards so that moved loops won't get processed twice, as long as time is moving forwards.
                for (int index = transformationCount - 1; index >= 0; index--)
                {
                    Transformation tr = transformations[index];

                    if (!tr.Loop)
                        continue;

                    if (tr.MaxLoopCount == 0 && !updateEndlessLoops)
                        continue;

                    int loopsToDo = 0;
                    double transformationDuration = tr.Duration;
                    double loopDuration = transformationDuration + tr.LoopDelay;

                    if (tr.Time2 <= time)
                    {
                        int remainingLoops = tr.MaxLoopCount - tr.CurrentLoopCount - 1;
                        if (tr.MaxLoopCount > 0 && remainingLoops < 1)
                            continue;

                        // Move the loop forwards
                        loopsToDo = loopDuration == 0 ? 0 : (int)((time - tr.Time1) / loopDuration);
                        if (tr.MaxLoopCount > 0)
                            loopsToDo = Math.Min(loopsToDo, remainingLoops);
                    }
                    else if (tr.Time1 > time)
                    {
                        int rewindableLoops = tr.CurrentLoopCount;
                        if (rewindableLoops < 1)
                            continue;

                        // Rewind the loop
                        loopsToDo = (int)((time - tr.Time2 - tr.LoopDelay) / loopDuration);
                        loopsToDo = Math.Max(-rewindableLoops, loopsToDo);
                    }

                    if (loopsToDo != 0)
                    {
                        if (!transformationsChanged)
                            keepSorted = transformations.SequenceEqual(transformations.OrderBy(t => t));

                        tr.Time1 = tr.Time1 + loopDuration * loopsToDo;
                        tr.Time2 = tr.Time1 + transformationDuration;
                        tr.CurrentLoopCount += loopsToDo;

                        if (keepSorted)
                        {
                            // Keep transformations sorted if they are, this is faster than sorting them all at the end.
                            transformations.RemoveAt(index);
                            int insertedIndex = transformations.AddInPlace(tr);

                            if (insertedIndex < index)
                                index++;
                        }

                        transformationsChanged = true;
                    }
                }

                if (transformationsChanged)
                {
                    // Transformations need to be updated again, loops won't need to be updated as the current time stays the same.
                    if (!keepSorted)
                        transformations.Sort();
                    updateTransformations();
                    return;
                }
            }

            //dispose of past sprites
            if (!hasFuture && !shouldDraw)
                return;

            if (!(hasFuture && hasPast) && !shouldDraw)
                //not a current sprite. will not be drawn.
                return;

            //apply past transformations
            for (int i = transformationCount - 1; i >= 0; i--)
            {
                Transformation t = transformations[i];
                if (t.Time2 >= time) continue;

                shouldDraw = true;

                switch (t.Type)
                {
                    case TransformationType.Fade:
                        if (hasFade) break;
                        Alpha = t.EndFloat;
                        hasFade = true;
                        break;
                    case TransformationType.Movement:
                        if (hasMovement || hasMovementX || hasMovementY) break;
                        Position = t.EndVector;
                        hasMovement = true;
                        break;
                    case TransformationType.MovementX:
                        if (hasMovementX) break;
                        Position = new Vector2(t.EndFloat, Position.Y);
                        hasMovementX = true;
                        break;
                    case TransformationType.MovementY:
                        if (hasMovementY) break;
                        Position = new Vector2(Position.X, t.EndFloat);
                        hasMovementY = true;
                        break;
                    case TransformationType.Scale:
                        if (hasScale) break;
                        Scale = t.EndFloat;
                        hasScale = true;
                        break;
                    case TransformationType.VectorScale:
                        if (hasScale) break;
                        VectorScale = t.EndVector;
                        hasScale = true;
                        break;
                    case TransformationType.Rotation:
                        if (hasRotation) break;
                        Rotation = t.EndFloat;
                        hasRotation = true;
                        break;
                    case TransformationType.Colour:
                        if (hasColour) break;
                        Colour = t.EndColour;
                        hasColour = true;
                        break;
                    case TransformationType.ParameterAdditive:
                        if (!additive.HasValue)
                        {
                            bool isPermanent = t.Duration == 0;
                            additive = Additive = isPermanent;
                        }
                        break;
                    case TransformationType.ParameterFlipHorizontal:
                        if (!horizontalFlip.HasValue)
                        {
                            bool isPermanent = t.Duration == 0;
                            horizontalFlip = FlipHorizontal = isPermanent;
                        }
                        break;
                    case TransformationType.ParameterFlipVertical:
                        if (!verticalFlip.HasValue)
                        {
                            bool isPermanent = t.Duration == 0;
                            verticalFlip = FlipVertical = isPermanent;
                        }
                        break;
                }

                //remove any old past transformations for long-life sprites
                if (/*Clock == Clocks.Game &&*/ !t.Loop && LifetimeEnd == Double.MaxValue)
                {
                    transformations.RemoveAt(i);
                    transformationCount--;
                }
            }

            //apply future transformations as a last resort.
            if (hasFuture /*&& Clock != Clocks.Game*/)
            {
                for (int i = 0; i < transformationCount; i++)
                {
                    Transformation t = transformations[i];
                    if (t.Time1 < time) continue;

                    switch (t.Type)
                    {
                        case TransformationType.Fade:
                            if (hasFade) break;
                            Alpha = t.StartFloat;
                            hasFade = true;
                            break;
                        case TransformationType.Movement:
                            if (hasMovement || hasMovementX || hasMovementY) break;
                            Position = t.StartVector;
                            hasMovement = true;
                            break;
                        case TransformationType.MovementX:
                            if (hasMovementX) break;
                            hasMovementX = true;
                            Position = new Vector2(t.StartFloat, Position.Y);
                            break;
                        case TransformationType.MovementY:
                            if (hasMovementY) break;
                            hasMovementY = true;
                            Position = new Vector2(Position.X, t.StartFloat);
                            break;
                        case TransformationType.Scale:
                            if (hasScale) break;
                            Scale = t.StartFloat;
                            hasScale = true;
                            break;
                        case TransformationType.VectorScale:
                            if (hasScale) break;
                            VectorScale = t.StartVector;
                            hasScale = true;
                            break;
                        case TransformationType.Rotation:
                            if (hasRotation) break;
                            Rotation = t.StartFloat;
                            hasRotation = true;
                            break;
                        case TransformationType.Colour:
                            if (hasColour) break;
                            Colour = t.StartColour;
                            hasColour = true;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Invalidates draw matrix and autosize caches.
        /// </summary>
        /// <returns>If the invalidate was actually necessary.</returns>
        public virtual bool Invalidate(bool affectsSize = true, bool affectsPosition = true, Drawable source = null)
        {
            if (affectsPosition && source != Parent && Parent?.ChildrenShouldInvalidate == true)
                Parent.Invalidate(affectsPosition, affectsPosition, this);

            bool alreadyInvalidated = true;

            //todo: some of these can be skipped depending on the invalidation type.
            alreadyInvalidated &= !boundingSizeBacking.Invalidate();
            alreadyInvalidated &= !drawInfoBacking.Invalidate();
            alreadyInvalidated &= !screenSpaceDrawQuadBacking.Invalidate();
            alreadyInvalidated &= !boundingSizeBacking.Invalidate();

            if (alreadyInvalidated) return false;

            if (Children != null)
            {
                foreach (var c in Children)
                {
                    if (c == source) continue;
                    c.Invalidate(false, affectsPosition, this);
                }
            }

            return true;
        }

        protected Vector2 GetAnchoredPosition(Vector2 pos)
        {
            if (!HasDefinedSize || Anchor == Anchor.TopLeft)
                return pos;

            Vector2 parentSize = Parent?.ActualSize ?? Vector2.Zero;

            if ((Anchor & Anchor.x1) > 0)
                pos.X += parentSize.X / 2f;
            else if ((Anchor & Anchor.x2) > 0)
                pos.X = parentSize.X - pos.X;

            if ((Anchor & Anchor.y1) > 0)
                pos.Y += parentSize.Y / 2f;
            else if ((Anchor & Anchor.y2) > 0)
                pos.Y = parentSize.Y - pos.Y;

            return pos;
        }

        private void updateDepthChanges()
        {
            while (depthChangeQueue.Count > 0)
            {
                Drawable childToResort = depthChangeQueue.Dequeue();

                internalChildren.Remove(childToResort);
                internalChildren.Add(childToResort);
            }
        }

        ~Drawable()
        {
            if (Game != null)
                //todo: check this scheduler call is actually required.
                Game.Scheduler.Add(() => Dispose(false));
            else
                Dispose(false);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;
            isDisposed = true;

            Parent = null;

            Clear();

            if (IsDisposable)
                OnUpdate = null;
        }

        public override string ToString()
        {
            string shortClass = base.ToString();
            shortClass = shortClass.Substring(shortClass.LastIndexOf('.') + 1);
            if (HasDefinedSize)
                return $@"{shortClass} pos {Position} size {Size}";
            else
                return $@"{shortClass} pos {Position} size -uncalculated-";
        }

        public virtual Drawable Clone()
        {
            Drawable thisNew = (Drawable)MemberwiseClone();

            thisNew.internalChildren = new LifetimeList<Drawable>(DepthComparer);
            Children.ForEach(c => thisNew.internalChildren.Add(c.Clone()));

            thisNew.transformations = Transformations.Select(t => t.Clone()).ToList();

            thisNew.drawInfoBacking.Invalidate();
            thisNew.boundingSizeBacking.Invalidate();

            return thisNew;
        }

        protected Game Game;

        protected virtual bool ChildrenShouldInvalidate => false;

        [Conditional("DEBUG")]
        private void ensureMainThread()
        {
            //This check is very intrusive on performance, so let's only run when a debugger is actually attached.
            if (!Debugger.IsAttached) return;

            //We can skip this check if this drawable isn't added to a rooted draw tree.
            //This allows creating nested drawables on a different thread, then scheduling them to
            //be added to a rooted tree for actual use.
            if (Parent == null)
                return;

            Debug.Assert(Game.MainThread == Thread.CurrentThread);
        }
    }

    /// <summary>
    /// General enum to specify an "anchor" or "origin" point from the standard 9 points on a rectangle.
    /// x and y counterparts can be accessed using bitwise flags.
    /// </summary>
    [Flags]
    public enum Anchor
    {
        TopLeft = y0 | x0,
        TopCentre = y0 | x1,
        TopRight = y0 | x2,

        CentreLeft = y1 | x0,
        Centre = y1 | x1,
        CentreRight = y1 | x2,

        BottomLeft = y2 | x0,
        BottomCentre = y2 | x1,
        BottomRight = y2 | x2,

        /// <summary>
        /// The vertical counterpart is at "Top" position.
        /// </summary>
        y0 = 0,
        /// <summary>
        /// The vertical counterpart is at "Centre" position.
        /// </summary>
        y1 = 1,
        /// <summary>
        /// The vertical counterpart is at "Bottom" position.
        /// </summary>
        y2 = 2,

        /// <summary>
        /// The horizontal counterpart is at "Left" position.
        /// </summary>
        x0 = 0,
        /// <summary>
        /// The horizontal counterpart is at "Centre" position.
        /// </summary>
        x1 = 4,
        /// <summary>
        /// The horizontal counterpart is at "Right" position.
        /// </summary>
        x2 = 8,

        /// <summary>
        /// The user is manually updating the outcome, so we shouldn't.
        /// </summary>
        Custom = 32,
    }

    public class DepthComparer : IComparer<Drawable>
    {
        public int Compare(Drawable x, Drawable y)
        {
            if (x.Depth == y.Depth) return -1;
            return x.Depth.CompareTo(y.Depth);
        }
    }

    public class DepthComparerReverse : IComparer<Drawable>
    {
        public int Compare(Drawable x, Drawable y)
        {
            if (x.Depth == y.Depth) return 1;
            return x.Depth.CompareTo(y.Depth);
        }
    }

    [Flags]
    public enum InheritMode
    {
        None = 0,

        X = 1 << 0,
        Y = 1 << 1,

        XY = X | Y
    }
}
