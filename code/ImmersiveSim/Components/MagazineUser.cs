#nullable enable

namespace ImmersiveSim.Components;

public class MagazineUser : Component
{
	[Property] public required string AmmoType { get; set; }
	[Property] public MagazineComponent? LoadedMagazine { get; set; }

	public void LoadMagazine( Player player, MagazineComponent magazine )
	{
		UnloadToPlayerInventory( player );

		LoadedMagazine = magazine;
		player.inventory.Remove( magazine.GameObject );

		Sound.Play( "reload" );
	}

	public void UnloadToPlayerInventory( Player player )
	{
		if ( LoadedMagazine == null )
		{
			return;
		}

		player.inventory.Add( LoadedMagazine.GameObject );
		LoadedMagazine = null;
	}

	public void UnloadToWorld()
	{
		if ( LoadedMagazine == null )
		{
			return;
		}

		LoadedMagazine.GetComponent<Collectable>().DropFromWorld( WorldPosition );
		LoadedMagazine = null;
	}

	public bool TryReloadFromPlayerInventory( Player player )
	{
		var eligibleMagazine = player.inventory.Select( item => item.GetComponent<MagazineComponent>() ).Where(
			magazine => magazine != null && magazine.AmmoType == AmmoType &&
			            magazine != LoadedMagazine &&
			            magazine.AmmoCount != 0 ).FirstOrDefault( (MagazineComponent?)null );

		if ( eligibleMagazine == null )
		{
			return false;
		}

		LoadMagazine( player, eligibleMagazine );
		return true;
	}
};
