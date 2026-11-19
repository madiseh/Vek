namespace Vek
{
    /// <summary>
    /// Defines when a text field commits user-entered text to its bound value.
    /// </summary>
    public enum BindValueMode
    {
        /// <summary>
        /// Commits each input event immediately.
        /// </summary>
        OnInput,

        /// <summary>
        /// Commits the current text when Enter is pressed or the field loses focus.
        /// </summary>
        OnChange,

        /// <summary>
        /// Commits after the user stops typing for the configured delay, or immediately on Enter or blur.
        /// </summary>
        OnDelayedInput
    }
}
