#nullable enable

namespace ImmersiveSim.UISystem;

public class InteractionReticleParams
{
	/// <summary>
	/// Factor to determine the length of the corners, use `1.0` for a full rectangle
	/// </summary>
	[Property] public float CornerFactor { get; set; } = 0.10f;

	/// <summary>
	/// Radius of the corner rounding
	/// </summary>
	[Property] public float CornerRounding { get; set; } = 10.0f;

	/// <summary>
	/// Thickness of the lines
	/// </summary>
	[Property] public float LineThickness { get; set; } = 2.5f;

	/// <summary>
	/// Color of the reticle
	/// </summary>
	[Property] public Color Color { get; set; } = Color.White;

	/// <summary>
	/// Configuration of the display text (main text), provided by `Interactable`
	/// </summary>
	[Property]
	public TextRendering.Scope DisplayTextScope { get; set; } = new( "", Color.White, 24.0f );

	/// <summary>
	/// Configuration of the details text, provided by `IReticleTextProvider`
	/// </summary>
	[Property]
	public TextRendering.Scope DetailsTextScope { get; set; } = new( "", Color.White, 18.0f );
};
