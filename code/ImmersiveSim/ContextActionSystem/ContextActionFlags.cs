#nullable enable

using System;

namespace ImmersiveSim.ContextActionSystem;

[Flags]
public enum ContextActionFlags
{
	None = 0,
	AvailableInContextMenu = 1 << 0,
	AvailableInInventory = 1 << 1,
	ClosesContextMenuOnUse = 1 << 2,
	ClosesInventoryOnUse = 1 << 3
}
