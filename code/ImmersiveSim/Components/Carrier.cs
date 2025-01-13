#nullable enable

using ImmersiveSim.UISystem;

namespace ImmersiveSim.Components;

public record CarryState( Rigidbody Body, Transform Transform, Transform OriginalTransform );

public class Carrier : Component, IHUDTextProvider
{
	[Property] public required Player Player { get; set; }
	[Property] public required PlayerController PlayerController { get; set; }

	[Property] public required float MinCarryDistance { get; set; } = 30.0f;
	[Property] public required float MaxCarryDistance { get; set; } = 50.0f;

	private float _carryDistance = 30.0f;
	private CarryState? _carryState;

	public Carriable? CarriedObject => _carryState?.Body.GetComponent<Carriable>() ?? null;

	public void StartCarrying( Rigidbody rigidBody )
	{
		_carryState = new CarryState
		(
			rigidBody,
			PlayerController.EyeTransform.ToLocal( rigidBody.Transform.World ),
			rigidBody.Transform.World
		);
	}

	public void StopCarrying()
	{
		_carryState = null;
	}

	protected override void OnUpdate()
	{
		if ( Input.Pressed( "Drop" ) )
		{
			StopCarrying();
			return;
		}

		_carryDistance += Time.Delta * Input.MouseWheel.y * 300.0f;
		_carryDistance = _carryDistance.Clamp( MinCarryDistance, MaxCarryDistance );
	}

	protected override void OnFixedUpdate()
	{
		if ( _carryState == null || !_carryState.Body.IsValid() )
		{
			return;
		}

		var rayEnd = PlayerController.EyePosition + Player.InteractionRayDirection * _carryDistance;

		var targetTransform = PlayerController.EyeTransform.ToWorld( _carryState.Transform );
		targetTransform.Position = rayEnd;
		targetTransform.Rotation = _carryState.OriginalTransform.Rotation.Angles()
			.WithYaw( targetTransform.Rotation.Angles().yaw );

		var distance = Vector3.DistanceBetween( targetTransform.Position, _carryState.Body.WorldPosition );

		if ( distance > MaxCarryDistance * 1.25f )
		{
			StopCarrying();
			return;
		}

		var mass = _carryState.Body.PhysicsBody.Mass;
		var moveSpeed = mass.Remap( 50, 3000, 0.05f, 2.0f, true );

		_carryState.Body.PhysicsBody.SmoothMove( targetTransform, moveSpeed, Time.Delta );
	}

	public string GetHUDText()
	{
		return _carryState == null ? "" : $"Carrying: '{_carryState.Body.GetComponent<Interactable>().DisplayName}'";
	}
}
