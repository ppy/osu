//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Graphics.Textures;
using osu.Framework.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.Graphics.Containers
{
    public class ProcessingContainer : Container
    {
        private Container processingContainer = new Container() { SizeMode = InheritMode.XY };

        public override void Load()
        {
            base.Load();

            AddTopLevel(processingContainer);
        }

        /// <summary>
        /// Adds a container and sets it as the new processing container.
        /// Any .Add() calls will be redirected to this container.
        /// </summary>
        /// <param name="container">The container to be the new processing container.</param>
        protected void AddProcessingContainer(Container container)
        {
            Debug.Assert(container != null);

            // If the current container is a processing conatiner then we only need
            // to add the new container to it (handled below)
            if (!(processingContainer is ProcessingContainer))
            {
                // Move existing children to the new container
                List<Drawable> existingChildren = processingContainer.Children.ToList();
                existingChildren.ForEach(child => container.Add(child));
            }

            processingContainer.Add(container);
            processingContainer = container;
        }

        /// <summary>
        /// Adds a container at the same level as the first processing container.
        /// </summary>
        /// <param name="drawable">The drawable to add.</param>
        /// <returns>The added drawable.</returns>
        protected Drawable AddTopLevel(Drawable drawable) => base.Add(drawable);

        /// <summary>
        /// Adds a drawable to the processing container.
        /// </summary>
        /// <param name="drawable">The drawable to add.</param>
        /// <returns>The added drawable.</returns>
        public override Drawable Add(Drawable drawable)
        {
            return processingContainer.Add(drawable);
        }

        /// <summary>
        /// Clears the processing container.
        /// </summary>
        /// <param name="dispose">Whether to dispose contained drawables.</param>
        public override void Clear(bool dispose = true)
        {
            processingContainer.Clear(dispose);
        }

        /// <summary>
        /// Removes a drawable from the processing container.
        /// </summary>
        /// <param name="drawable">The drawable to remove.</param>
        /// <param name="dispose">Whether to dispose the drawable.</param>
        /// <returns>Whether the drawable was removed.</returns>
        public override bool Remove(Drawable drawable, bool dispose = true)
        {
            return processingContainer.Remove(drawable, dispose);
        }
    }
}
