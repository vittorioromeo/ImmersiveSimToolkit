#nullable enable
using ImmersiveSim.UISystem;

namespace ImmersiveSim.Components;

public class AmmoBag : Interactable, IInventoryTextProvider, IReticleTextProvider
{
	[Property] public required string AmmoType { get; set; }
	[Property] public int AmmoCount { get; set; }

	public string GetInventoryText() => $"{AmmoType} | {AmmoCount}";
	public string GetReticleText() => GetInventoryText();
}
