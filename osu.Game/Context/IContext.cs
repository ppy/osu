namespace osu.Game.Context;

public interface IContext
{
    /// <summary>
    /// Makes a deep copy of this context.
    /// </summary>
    /// <returns>The deep copy of this context.</returns>
    public IContext Copy();
}
