#nullable enable

using ImmersiveSim.ContextActionSystem;
using ImmersiveSim.EquipmentSystem;
using ImmersiveSim.UISystem;

namespace ImmersiveSim.Components;

public class Pistol9mm : Component, IEquipmentBehavior, IContextActionProvider, IInventoryTextProvider,
	IReticleTextProvider
{
	public void OnEquip( Player player )
	{
	}

	public void OnUnequip( Player player )
	{
	}

	public void OnAttack1( Player player )
	{
		var magazineUser = GetComponent<MagazineUser>();

		if ( magazineUser.LoadedMagazine == null || magazineUser.LoadedMagazine.AmmoCount == 0 )
		{
			Sound.Play( "click" );
			return;
		}

		Sound.Play( "pew" );
		--magazineUser.LoadedMagazine.AmmoCount;

		// TODO:
		var playerController = player.GetComponent<PlayerController>();

		var rayStart = playerController.EyePosition;
		var rayTrace = Scene.Trace
			.Ray( new Ray( rayStart, player.InteractionRayDirection ), 1200.0f )
			.UsePhysicsWorld( true ).UseHitboxes( true ).Run();

		DebugOverlay.Line( rayStart + Vector3.Down * 2.0f, rayTrace.HitPosition, Color.Red, 1.0f );

		rayTrace.GameObject?.GetComponent<Prop>()?.GetComponent<Rigidbody>()
			?.ApplyImpulse( rayTrace.Direction * 10000.0f );
	}

	public void OnAttack2( Player player )
	{
	}

	public void OnUse( Player player )
	{
	}

	public void OnReload( Player player )
	{
		if ( GetComponent<MagazineUser>().TryReloadFromPlayerInventory( player ) )
		{
			Sound.Play( "reload" );
		}
	}

	public void OnDrop( Player player )
	{
	}

	public ContextAction[] GetContextActions( Player player ) =>
		GetComponent<MagazineUser>().LoadedMagazine != null
			?
			[
				new ContextAction( "Unload",
					ContextActionFlags.AvailableInContextMenu | ContextActionFlags.AvailableInInventory )
			]
			: [];

	public void OnContextAction( ContextActionSource contextActionSource, string contextActionName, Player player )
	{
		var magazineUser = GetComponent<MagazineUser>();

		if ( contextActionName == "Unload" )
		{
			if ( contextActionSource == ContextActionSource.Menu )
			{
				Sound.Play( "reload" );
				magazineUser.UnloadToWorld();
			}
			else
			{
				Sound.Play( "reload" );
				magazineUser.UnloadToPlayerInventory( player );
			}
		}
	}

	public string GetAmmoType() => "9mm";

	public string GetHUDText()
	{
		var magazineUser = GetComponent<MagazineUser>();

		return magazineUser.LoadedMagazine == null
			? $"{GetAmmoType()} | Unloaded"
			: $"{GetAmmoType()} | {magazineUser.LoadedMagazine.AmmoCount}/{magazineUser.LoadedMagazine.MaxAmmoCount}";
	}

	public string GetInventoryText() => GetHUDText();
	public string GetReticleText() => GetHUDText();
}
