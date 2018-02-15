namespace osu.Game.Database
{
    /// <summary>
    /// A class which can accept files for importing.
    /// </summary>
    public interface ICanAcceptFiles
    {
        /// <summary>
        /// Import the specified paths.
        /// </summary>
        /// <param name="paths">The files which should be imported.</param>
        void Import(params string[] paths);

        /// <summary>
        /// An array of accepted file extensions (in the standard format of ".abc").
        /// </summary>
        string[] HandledExtensions { get; }
    }
}
