//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

namespace osu.Framework.Audio.Sample
{
    public abstract class AudioSample : AdjustableAudioComponent, IHasCompletedState, IUpdateable
    {
        protected bool WasStarted;

        /// <summary>
        /// Makes this sample fire-and-forget (will be cleaned up automatically).
        /// </summary>
        public bool OneShot;

        public virtual void Play(bool restart = true)
        {
            WasStarted = true;
        }

        public virtual void Stop()
        {
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
            base.Dispose(disposing);
        }

        public abstract bool Playing { get; }

        public virtual bool Played => WasStarted && !Playing;

        public bool HasCompleted => Played && (OneShot || IsDisposed);

        public virtual void Pause()
        {
            if (!Playing) return;
        }
    }
}
