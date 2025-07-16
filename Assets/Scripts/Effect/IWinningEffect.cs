using DG.Tweening;

namespace MCRGame.Effect
{
    /// <summary>
    /// Interface for winning screen visual effects.
    /// Returns the DOTween sequence representing the effect so callers
    /// can chain additional logic.
    /// </summary>
    public interface IWinningEffect
    {
        /// <summary>
        /// Start the visual effect and return the sequence driving it.
        /// </summary>
        Sequence PlayEffect();
    }
}