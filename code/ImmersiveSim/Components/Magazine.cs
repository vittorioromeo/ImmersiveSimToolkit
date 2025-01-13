#nullable enable

using System;
using ImmersiveSim.ContextActionSystem;
using ImmersiveSim.UISystem;

namespace ImmersiveSim.Components;

public class MagazineComponent : Interactable, IContextActionProvider, IInventoryTextProvider, IReticleTextProvider
{
	[Property] public required string AmmoType { get; set; }
	[Property] public int AmmoCount { get; set; }
	[Property] public int MaxAmmoCount { get; set; }

	public ContextAction[] GetContextActions( Player player )
	{
		List<ContextAction> actions = [];

		if ( AmmoCount > 0 )
		{
			actions.Add( new ContextAction( "Empty", ContextActionFlags.AvailableInInventory ) );
		}

		if ( AmmoCount < MaxAmmoCount )
		{
			var ammoBag =
				player.FindFirstComponentInInventoryMatching<AmmoBag>( c => c?.AmmoType == AmmoType );

			if ( ammoBag?.AmmoCount > 0 )
			{
				actions.Add( new ContextAction( "Fill", ContextActionFlags.AvailableInInventory ) );
			}
		}

		var ammoUser = player.equippable?.GetComponent<MagazineUser>();
		if ( ammoUser != null && ammoUser.AmmoType == AmmoType )
		{
			actions.Add( new ContextAction( "Load",
				ContextActionFlags.AvailableInContextMenu | ContextActionFlags.AvailableInInventory |
				ContextActionFlags.ClosesContextMenuOnUse ) );
		}

		return actions.ToArray();
	}

	public string GetInventoryText() => $"{AmmoType} | {AmmoCount}/{MaxAmmoCount}";
	public string GetReticleText() => GetInventoryText();

	public void OnContextAction( ContextActionSource contextActionSource, string contextActionName, Player player )
	{
		if ( contextActionName == "Empty" )
		{
			var ammoBag =
				player.FindFirstComponentInInventoryMatching<AmmoBag>( c => c?.AmmoType == AmmoType );

			if ( ammoBag == null )
			{
				var ammoBagObj = Scene.GetPrefab( "immersivesimammobagprefab.prefab" ).Clone();
				ammoBagObj.Enabled = true;
				ammoBagObj.GetComponent<Collectable>().Collect( player );
				ammoBag = ammoBagObj.GetComponent<AmmoBag>();
			}

			ammoBag.AmmoCount += AmmoCount;
			AmmoCount = 0;
		}
		else if ( contextActionName == "Fill" )
		{
			foreach ( var item in player.inventory )
			{
				var ammoBag = player.FindFirstComponentInInventoryMatching<AmmoBag>(
					c => c?.AmmoType == AmmoType && c?.AmmoCount > 0 );

				if ( ammoBag == null )
				{
					continue;
				}

				var requiredAmmo = MaxAmmoCount - AmmoCount;
				var usableAmmo = Math.Min( requiredAmmo, ammoBag.AmmoCount );

				ammoBag.AmmoCount -= usableAmmo;
				AmmoCount += usableAmmo;
				break;
			}
		}
		else if ( contextActionName == "Load" )
		{
			if ( contextActionSource == ContextActionSource.Menu )
			{
				GetComponent<Collectable>().Collect( player );
			}

			player.equippable?.GetComponent<MagazineUser>()?.LoadMagazine( player, this );
		}
	}
}
