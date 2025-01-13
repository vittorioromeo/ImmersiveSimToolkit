#nullable enable

using System;
using ImmersiveSim.Components;
using ImmersiveSim.UISystem;

namespace ImmersiveSim;

public sealed class Player : Component
{
	//
	// Interaction
	[Property] public float InteractionRange { get; set; } = 120.0f;

	//
	// Interaction reticle
	[Property] public InteractionReticleParams InteractionReticleParams { get; set; } = new();

	//
	// Linked components and game objects
	[Property] public required PlayerController PlayerController { get; set; }
	[Property] public required GameObject UI { get; set; }

	//
	// Equipment
	public Equipment? equippable;

	//
	// Inventory
	public List<GameObject> inventory = new();

	public bool InMouseMode = false;

	public void SetMouseMode( bool enabled )
	{
		InMouseMode = enabled;

		Mouse.Visible = enabled;
		PlayerController.UseInputControls = !enabled;

		UI.GetComponent<UI.ContextMenu>( true ).Enabled = enabled;
		UI.GetComponent<UI.Inventory>( true ).Enabled = enabled;
		UI.GetComponent<UI.Crosshair>( true ).Enabled = !enabled;
	}

	private void OnUpdate_MouseMode()
	{
		if ( Input.Pressed( "mouseMode" ) )
		{
			SetMouseMode( !InMouseMode );
			UI.GetComponent<UI.ContextMenu>( true ).Enabled = false; // Close context menu
		}
	}

	private void OnUpdate_Interaction()
	{
		var rayTrace = Scene.Trace.Ray(
				new Ray( PlayerController.EyePosition, InteractionRayDirection ),
				InteractionRange )
			.UsePhysicsWorld( true ).UseHitboxes( true ).Run();

		var rayTraceGameObject = rayTrace.GameObject;
		if ( rayTraceGameObject == null ) { return; }

		var interactable = rayTraceGameObject.GetComponent<Interactable>();
		if ( interactable == null ) { return; }

		var modelRenderer = rayTraceGameObject.GetComponent<ModelRenderer>();
		if ( modelRenderer == null ) { return; }

		Vector2[] points = Utils.DrawScreenSpaceReticle( modelRenderer.Model.Bounds, rayTraceGameObject.WorldTransform,
			Scene.Camera, InteractionReticleParams );

		//
		// Draw display text
		var hud = Scene.Camera.Hud;
		var displayTextScope = InteractionReticleParams.DisplayTextScope;
		displayTextScope.Text = interactable.DisplayName;
		displayTextScope.FontName = "Segoe UI";
		hud.DrawText( displayTextScope, new Rect( points[3] ), TextFlag.Left );

		var detailsTextScope = InteractionReticleParams.DetailsTextScope;

		string reticleText = interactable.Components.GetAll().OfType<IReticleTextProvider>()
			.Aggregate( "", ( acc, provider ) => acc + provider.GetReticleText() );
		detailsTextScope.Text = reticleText;

		detailsTextScope.FontName = "Segoe UI";
		hud.DrawText( detailsTextScope, new Rect( points[3] + Vector2.Up * displayTextScope.FontSize * 1.15f ),
			TextFlag.Left );

		//
		// Handle context menu
		if ( Input.Pressed( "attack2" ) )
		{
			var menuPosition = InMouseMode ? Mouse.Position : Screen.Size * 0.5f;

			if ( !InMouseMode )
			{
				Mouse.Position = menuPosition;
			}

			SetMouseMode( true );

			var contextMenu = UI.GetComponent<UI.ContextMenu>();

			contextMenu.DisplayName = interactable.DisplayName;
			contextMenu.Player = this;
			contextMenu.Interactable = interactable;

			contextMenu.Enabled = true;

			var scaleFactor = UI.GetComponent<ScreenPanel>().GetPanel().ScaleToScreen;
			contextMenu.Panel.Style.Left = menuPosition.x / scaleFactor;
			contextMenu.Panel.Style.Top = menuPosition.y / scaleFactor;
		}
	}

	public Vector3 InteractionRayDirection =>
		InMouseMode
			? (Scene.Camera.ScreenToWorld( Mouse.Position ) - PlayerController.EyePosition).Normal
			: PlayerController.EyeTransform.Forward;

	private void OnUpdate_Equipment()
	{
		var hud = Scene.Camera.Hud;

		var displayTextScope = InteractionReticleParams.DisplayTextScope;
		displayTextScope.FontSize = 16;

		// TODO:
		if ( equippable != null )
		{
			var interactable = equippable.GetComponent<Interactable>();
			displayTextScope.Text = "Equipped: " + interactable.DisplayName + equippable.GetHUDText();
		}

		// TODO:
		foreach ( var hudTextProvider in Components.GetAll().OfType<IHUDTextProvider>() )
		{
			displayTextScope.Text += "\n" + hudTextProvider.GetHUDText();
		}

		hud.DrawText( displayTextScope, new Rect( new Vector2( 10, Screen.Height - 10 ) ), TextFlag.LeftBottom );

		if ( equippable == null )
		{
			return;
		}

		if ( Input.Pressed( "attack1" ) ) { equippable.OnAttack1( this ); }

		if ( Input.Pressed( "attack2" ) ) { equippable.OnAttack2( this ); }

		if ( Input.Pressed( "use" ) ) { equippable.OnUse( this ); }

		if ( Input.Pressed( "reload" ) ) { equippable.OnReload( this ); }

		if ( Input.Pressed( "Drop" ) ) { equippable.OnDrop( this ); }
	}

	protected override void OnUpdate()
	{
		var dropPressed = Input.Pressed( "Drop" );

		if ( dropPressed )
		{
			if ( equippable != null )
			{
				StopEquipping();
				return;
			}
		}

		OnUpdate_MouseMode();
		OnUpdate_Interaction();
		OnUpdate_Equipment();
	}

	//
	// Equipment
	public void StartEquipping( Equipment newEquippable )
	{
		if ( equippable == newEquippable )
		{
			return;
		}

		StopEquipping();

		equippable = newEquippable;

		equippable.OnEquip( this );
		equippable.GetComponent<Prop>( true ).Enabled = false;
	}

	public void StopEquipping()
	{
		equippable?.OnUnequip( this );
		equippable = null;
	}

	//
	// Inventory
	public T? FindFirstComponentInInventoryMatching<T>( Predicate<T> predicate ) where T : Component =>
		inventory.Select( item => item.GetComponent<T>() )
			.FirstOrDefault( component => component != null && predicate( component ) );
}
