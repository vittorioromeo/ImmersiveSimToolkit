#nullable enable

using ImmersiveSim.ContextActionSystem;
using ImmersiveSim.UISystem;

namespace ImmersiveSim.Components;

public class Replicator : Component, IReticleTextProvider
{
    private int _magsTimer = 0;

    public string GetReticleText() => "Needs charged battery";

    protected override void OnFixedUpdate()
    {
        if ( _magsTimer > 0 )
        {
            --_magsTimer;

            if ( _magsTimer % 10 == 0 )
            {
                Sound.Play( "magout" );

                var magazine = Scene.GetPrefab( "immersivesimmagazine9mm.prefab" ).Clone();
                magazine.WorldPosition = WorldPosition + WorldTransform.Forward * 15.0f + WorldTransform.Up * 15.0f;
                magazine.GetComponent<Rigidbody>().ApplyImpulse( WorldTransform.Forward * 500.0f + Vector3.Random * 250.0f );
            }
        }

        foreach ( var battery in Utils.FindNearbyComponents<Battery>( Scene, WorldPosition, 65.0f ) )
        {
            if ( !battery.Charged )
            {
                continue;
            }

            battery.GetComponent<Carriable>().LastCarrier?.StopCarrying();
            battery.DestroyGameObject();

            Sound.Play( "charge" );

            _magsTimer += 100;
        }
    }
}
