#nullable enable

using Sandbox;
using Sandbox.Rendering;
using System;
using System.Diagnostics;
using System.Numerics;
using ImmersiveSim.ContextActionSystem;
using ImmersiveSim.UISystem;

namespace ImmersiveSim;

/// <summary>
/// General utility functions
/// </summary>
public static class Utils
{
	/// <summary>
	/// Returns the four points of a bounding box in screen space for the given object
	/// </summary>
	/// <param name="localBounds">Local bounds of the object</param>
	/// <param name="gameObjectWorldTransform">World space transform of the object</param>
	/// <param name="camera">Player camera</param>
	/// <param name="offset">Offset for every point (outwards, from the center)</param>
	/// <returns>An array containing the four points</returns>
	public static Vector2[] GetScreenSpaceReticlePoints( BBox localBounds, Transform gameObjectWorldTransform,
		CameraComponent camera, float offset = 10.0f )
	{
		//
		// Create corners in local space
		Vector3[] localCorners =
		{
			new(localBounds.Mins.x, localBounds.Mins.y, localBounds.Mins.z),
			new(localBounds.Mins.x, localBounds.Mins.y, localBounds.Maxs.z),
			new(localBounds.Mins.x, localBounds.Maxs.y, localBounds.Mins.z),
			new(localBounds.Mins.x, localBounds.Maxs.y, localBounds.Maxs.z),
			new(localBounds.Maxs.x, localBounds.Mins.y, localBounds.Mins.z),
			new(localBounds.Maxs.x, localBounds.Mins.y, localBounds.Maxs.z),
			new(localBounds.Maxs.x, localBounds.Maxs.y, localBounds.Mins.z),
			new(localBounds.Maxs.x, localBounds.Maxs.y, localBounds.Maxs.z),
		};

		//
		// Transform each corner to world space
		var worldTransform = gameObjectWorldTransform;
		var worldCorners = new Vector3[8];
		for ( var i = 0; i < 8; i++ )
		{
			worldCorners[i] = worldTransform.PointToWorld( localCorners[i] );
		}

		//
		// Project to screen space
		var screenCorners = new Vector2[8];
		for ( var i = 0; i < 8; i++ )
		{
			screenCorners[i] = camera.PointToScreenPixels( worldCorners[i] );
		}

		//
		// pigure out min/max coordinates in screen space
		Vector2 min = screenCorners[0];
		Vector2 max = screenCorners[0];

		for ( var i = 1; i < screenCorners.Length; i++ )
		{
			min = Vector2.Min( min, screenCorners[i] );
			max = Vector2.Max( max, screenCorners[i] );
		}

		//
		// Get 2D bounding box points in screen space
		/*
			A-----B
			| 	  |
			|	  |
			D-----C
		*/

		return
		[
			new Vector2( min.x - offset, min.y - offset ), // A
			new Vector2( max.x + offset, min.y - offset ), // B
			new Vector2( max.x + offset, max.y + offset ), // C
			new Vector2( min.x - offset, max.y + offset ) // D
		];
	}

	/// <summary>
	/// Draws a screen space reticle for the given object
	/// </summary>
	/// <param name="localBounds">Local bounds of the object</param>
	/// <param name="gameObjectWorldTransform">World space transform of the object</param>
	/// <param name="camera">Player camera</param>
	/// <param name="interactionReticleParams">Parameters for the reticle</param>
	/// <returns></returns>
	public static Vector2[] DrawScreenSpaceReticle( BBox localBounds, Transform gameObjectWorldTransform,
		CameraComponent camera, InteractionReticleParams interactionReticleParams )
	{
		var points = GetScreenSpaceReticlePoints( localBounds, gameObjectWorldTransform, camera );

		var pointA = points[0];
		var pointB = points[1];
		var pointC = points[2];
		var pointD = points[3];

		var hud = camera.Hud;
		var thickness = interactionReticleParams.LineThickness;
		var rounding = new Vector4( interactionReticleParams.CornerRounding );
		var color = interactionReticleParams.Color;

		if ( interactionReticleParams.CornerFactor >= 1.0f )
		{
			hud.DrawLine( pointA, pointB, thickness, color, rounding );
			hud.DrawLine( pointB, pointC, thickness, color, rounding );
			hud.DrawLine( pointC, pointD, thickness, color, rounding );
			hud.DrawLine( pointD, pointA, thickness, color, rounding );
		}
		else
		{
			var horizontalLength = (pointA - pointB).Length;
			var verticalLength = (pointA - pointD).Length;
			var length = Math.Min( horizontalLength, verticalLength ) * interactionReticleParams.CornerFactor;

			hud.DrawLine( pointA, pointA + Vector2.Left * length, thickness, color, rounding );
			hud.DrawLine( pointA, pointA + Vector2.Up * length, thickness, color, rounding );

			hud.DrawLine( pointB, pointB + Vector2.Right * length, thickness, color, rounding );
			hud.DrawLine( pointB, pointB + Vector2.Up * length, thickness, color, rounding );

			hud.DrawLine( pointC, pointC + Vector2.Right * length, thickness, color, rounding );
			hud.DrawLine( pointC, pointC + Vector2.Down * length, thickness, color, rounding );

			hud.DrawLine( pointD, pointD + Vector2.Left * length, thickness, color, rounding );
			hud.DrawLine( pointD, pointD + Vector2.Down * length, thickness, color, rounding );
		}

		return [pointA, pointB, pointC, pointD];
	}

	/// <summary>
	/// Returns all context actions with the required flag from the given game object
	/// </summary>
	/// <param name="player">Current player</param>
	/// <param name="gameObject">Target game object</param>
	/// <param name="requiredFlag">Required context action flag</param>
	public static IEnumerable<(IContextActionProvider, ContextAction)> GetContextActionsWithFlag( Player player,
		GameObject gameObject, ContextActionFlags requiredFlag ) =>
		from contextActionProvider in gameObject.Components.GetAll().OfType<IContextActionProvider>()
		from contextAction in contextActionProvider.GetContextActions( player )
		where contextAction.Flags.HasFlag( requiredFlag )
		select (contextActionProvider, contextAction);

	/// <summary>
	/// Executes a context action from the UI
	/// </summary>
	/// <param name="player">Currnet player</param>
	/// <param name="contextActionSource">Source UI of the context action</param>
	/// <param name="contextActionProvider">Provider of the context action</param>
	/// <param name="contextAction">Context action to execute</param>
	public static void DoContextActionFromUI( Player player,
		ContextActionSource contextActionSource, IContextActionProvider contextActionProvider,
		ContextAction contextAction )
	{
		var closeMenuFlag = contextActionSource == ContextActionSource.Menu
			? ContextActionFlags.ClosesContextMenuOnUse
			: ContextActionFlags.ClosesInventoryOnUse;

		Sound.Play( "uiclick" );

		contextActionProvider.OnContextAction( contextActionSource, contextAction.Name, player );

		if ( contextAction.Flags.HasFlag( closeMenuFlag ) )
		{
			player.SetMouseMode( false );
		}
	}

	public static IEnumerable<T> FindNearbyComponents<T>( Scene scene, Vector3 worldPosition, float radius )
		where T : Component
	{
		foreach ( var obj in scene.FindInPhysics( new Sphere( worldPosition, radius ) ) )
		{
			if ( obj.GetComponent<T>() is { } component )
			{
				yield return component;
			}
		}
	}

	public static IEnumerable<T> ForEach<T>( this IEnumerable<T> enumeration, Action<T> action )
	{
		foreach ( var item in enumeration )
		{
			action( item );
		}

		return enumeration;
	}
}
