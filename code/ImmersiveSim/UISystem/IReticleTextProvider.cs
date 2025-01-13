#nullable enable

namespace ImmersiveSim.UISystem;

/// <summary>
/// Interface for providing text to be displayed in the reticle
/// </summary>
public interface IReticleTextProvider
{
    public string GetReticleText();
}
