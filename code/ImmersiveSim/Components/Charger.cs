#nullable enable

using ImmersiveSim.ContextActionSystem;
using ImmersiveSim.UISystem;

namespace ImmersiveSim.Components;

public class Charger : Component, IReticleTextProvider
{
    public string GetReticleText() => "Recharges nearby batteries";

    protected override void OnFixedUpdate()
    {
        foreach ( var battery in Utils.FindNearbyComponents<Battery>( Scene, WorldPosition, 25.0f ) )
        {
	        if ( battery.Charged )
	        {
		        continue;
	        }

	        battery.Charged = true;
	        Sound.Play( "charge" );
        }
    }
}
