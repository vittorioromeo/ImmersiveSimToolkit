#nullable enable

using ImmersiveSim.ContextActionSystem;
using ImmersiveSim.UISystem;

namespace ImmersiveSim.Components;

public class Battery : Component, IInventoryTextProvider, IReticleTextProvider
{
	[Property] public bool Charged { get; set; }

	public string GetInventoryText() => Charged ? "Charged" : "Uncharged";
	public string GetReticleText() => GetInventoryText();
}
