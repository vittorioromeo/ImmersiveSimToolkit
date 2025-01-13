#nullable enable

using ImmersiveSim.ContextActionSystem;

namespace ImmersiveSim.Components;

public class Carriable : Component, IContextActionProvider
{
	public Carrier? LastCarrier { get; set; }

	public ContextAction[] GetContextActions( Player player ) => player.GetComponent<Carrier>().CarriedObject == null
		?
		[
			new("Carry",
				ContextActionFlags.AvailableInContextMenu | ContextActionFlags.AvailableInInventory |
				ContextActionFlags.ClosesContextMenuOnUse)
		]
		: [new("Drop", ContextActionFlags.AvailableInContextMenu | ContextActionFlags.ClosesContextMenuOnUse)];

	public void OnContextAction( ContextActionSource contextActionSource, string contextActionName, Player player )
	{
		if ( contextActionName == "Carry" )
		{
			if ( contextActionSource == ContextActionSource.Inventory )
			{
				GetComponent<Collectable>().DropFromPlayerInventory( player );
			}

			LastCarrier = player.GetComponent<Carrier>();
			LastCarrier.StartCarrying( GetComponent<Rigidbody>() );
		}
		else if ( contextActionName == "Drop" )
		{
			LastCarrier?.StopCarrying();
		}
	}
}
