#nullable enable

using ImmersiveSim.UISystem;

namespace ImmersiveSim.EquipmentSystem;

public interface IEquipmentBehavior : IHUDTextProvider
{
	public void OnEquip( Player player );
	public void OnUnequip( Player player );
	public void OnAttack1( Player player );
	public void OnAttack2( Player player );
	public void OnUse( Player player );
	public void OnReload( Player player );
	public void OnDrop( Player player );
}
