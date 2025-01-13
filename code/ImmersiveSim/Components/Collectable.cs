#nullable enable

using ImmersiveSim.ContextActionSystem;

namespace ImmersiveSim.Components;

public class Collectable : Component, IContextActionProvider
{
	public ContextAction[] GetContextActions( Player player ) =>
	[
		new("Collect", ContextActionFlags.AvailableInContextMenu | ContextActionFlags.ClosesContextMenuOnUse),
		new("Drop", ContextActionFlags.AvailableInInventory),
	];

	public void Collect( Player player )
	{
		if ( GetComponent<Prop>() is { } prop )
		{
			prop.Enabled = false;
		}

		player.inventory.Add( GameObject );
	}

	public void DropFromPlayerInventory( Player player )
	{
		if ( GetComponent<Prop>( true ) is { } prop )
		{
			prop.Enabled = true;
		}

		if ( GetComponent<Equipment>() is { } equippable && player.equippable == equippable )
		{
			player.StopEquipping();
		}

		var playerController = player.GetComponent<PlayerController>();
		WorldPosition = playerController.EyePosition + playerController.EyeTransform.Forward * 25.0f;

		player.inventory.Remove( GameObject );
	}

	public void DropFromWorld( Vector3 worldPosition )
	{
		if ( GetComponent<Prop>( true ) is { } prop )
		{
			prop.Enabled = true;
		}

		WorldPosition = worldPosition;
	}

	public void OnContextAction( ContextActionSource contextActionSource, string contextActionName, Player player )
	{
		if ( contextActionName == "Collect" )
		{
			player.GetComponent<Carrier>().StopCarrying();
			Collect( player );
		}
		else if ( contextActionName == "Drop" )
		{
			player.GetComponent<Carrier>().StopCarrying();
			DropFromPlayerInventory( player );
		}
	}
}
