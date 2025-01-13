#nullable enable

namespace ImmersiveSim.UISystem;

/// <summary>
/// Interface for providing text to be displayed in the inventory
/// </summary>
public interface IInventoryTextProvider
{
	public string GetInventoryText();
}
