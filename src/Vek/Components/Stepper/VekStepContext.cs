namespace Vek;

/// <summary>
/// Provides contextual information about a <see cref="VekStep"/> within a <see cref="VekStepper"/>.
/// </summary>
public sealed class VekStepContext
{
    /// <summary>
    /// Gets the owning <see cref="VekStepper"/>.
    /// </summary>
    public VekStepper Stepper { get; }

    /// <summary>
    /// Gets the <see cref="VekStep"/> associated with the context.
    /// </summary>
    public VekStep Step { get; }

    /// <summary>
    /// Gets a value indicating whether the associated step is currently active.
    /// </summary>
    public bool IsActive => Stepper.ActiveStep == Step;

    /// <summary>
    /// Initializes a new instance of the <see cref="VekStepContext"/> class.
    /// </summary>
    /// <param name="stepper">The owning stepper.</param>
    /// <param name="step">The step associated with the context.</param>
    public VekStepContext(VekStepper stepper, VekStep step)
    {
        Stepper = stepper ?? throw new ArgumentNullException(nameof(stepper));
        Step = step ?? throw new ArgumentNullException(nameof(step));
    }
}
