#nullable enable

using System;
using ImmersiveSim.UI;
using ImmersiveSim.ContextActionSystem;
using ImmersiveSim.EquipmentSystem;
using ImmersiveSim.UISystem;

namespace ImmersiveSim.Components;

public class Equipment : Component, IContextActionProvider, IHUDTextProvider
{
	public ContextAction[] GetContextActions( Player player ) =>
		player.equippable == this
			?
			[
				new("Unequip", ContextActionFlags.AvailableInInventory)
			]
			:
			[
				new("Equip",
					ContextActionFlags.AvailableInContextMenu | ContextActionFlags.AvailableInInventory |
					ContextActionFlags.ClosesContextMenuOnUse),
			];


	public void OnContextAction( ContextActionSource contextActionSource, string contextActionName, Player player )
	{
		if ( contextActionName == "Equip" )
		{
			player.GetComponent<Carrier>().StopCarrying();
			
			if ( contextActionSource == ContextActionSource.Menu )
			{
				GetComponent<Collectable>().Collect( player );
				player.UI.GetComponent<Inventory>( true ).RebuildBump += 1;
				player.UI.GetComponent<ContextMenu>( true ).Enabled = false;
			}

			player.StartEquipping( this );
		}
		else if ( contextActionName == "Unequip" )
		{
			player.StopEquipping();
		}
	}

	private IEnumerable<IEquipmentBehavior> Behaviors => Components.GetAll().OfType<IEquipmentBehavior>();

	public void OnEquip( Player player ) { Behaviors.ForEach( b => b.OnEquip( player ) ); }
	public void OnUnequip( Player player ) { Behaviors.ForEach( b => b.OnUnequip( player ) ); }
	public void OnAttack1( Player player ) { Behaviors.ForEach( b => b.OnAttack1( player ) ); }
	public void OnAttack2( Player player ) { Behaviors.ForEach( b => b.OnAttack2( player ) ); }
	public void OnUse( Player player ) { Behaviors.ForEach( b => b.OnUse( player ) ); }
	public void OnReload( Player player ) { Behaviors.ForEach( b => b.OnReload( player ) ); }
	public void OnDrop( Player player ) { Behaviors.ForEach( b => b.OnDrop( player ) ); }

	public string GetHUDText()
	{
		return Behaviors.Aggregate( "", ( acc, b ) => acc + "\n" + b.GetHUDText() );
	}
}
