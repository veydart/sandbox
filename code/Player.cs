using Sandbox;

partial class SandboxPlayer : BasePlayer
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	private DamageInfo lastDamage;

	[Net]
	public PlayerController VehicleController { get; set; }

	[Net]
	public Camera VehicleCamera { get; set; }

	[Net]
	public PlayerAnimator VehicleAnimator { get; set; }

	public SandboxPlayer()
	{
		Inventory = new Inventory( this );
	}

	public override void Respawn()
	{
		SetModel( "models/citizen/citizen.vmdl" );

		Controller = new WalkController();
		Animator = new StandardPlayerAnimator();
		Camera = new FirstPersonCamera();

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		Dress();

		Inventory.Add( new PhysGun(), true );
		Inventory.Add( new GravGun() );
		Inventory.Add( new Tool() );
		Inventory.Add( new Gun() );

		base.Respawn();
	}

	public override void OnKilled()
	{
		base.OnKilled();

		Inventory.DropActive();
		Inventory.DeleteContents();

		BecomeRagdollOnClient( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

		Controller = null;
		Camera = new SpectateRagdollCamera();

		EnableAllCollisions = false;
		EnableDrawing = false;
	}

	public override void TakeDamage( DamageInfo info )
	{
		lastDamage = info;

		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

		base.TakeDamage( info );
	}

	[ClientRpc]
	public void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
	{
		if ( this == Local )
		{
			_ = new Sandbox.ScreenShake.Random( 1.5f, 1.0f, 2.5f );
		}
	}

	public override PlayerController GetActiveController()
	{
		if ( DevController != null ) return DevController;
		if ( VehicleController != null ) return VehicleController;

		return base.GetActiveController();
	}

	public override Camera GetActiveCamera()
	{
		if ( DevCamera != null ) return DevCamera;
		if ( VehicleCamera != null ) return VehicleCamera;

		return base.GetActiveCamera();
	}

	public override PlayerAnimator GetActiveAnimator()
	{
		if ( VehicleAnimator != null ) return VehicleAnimator;

		return base.GetActiveAnimator();
	}

	protected override void Tick()
	{
		base.Tick();

		if ( Input.ActiveChild != null )
		{
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		TickPlayerUse();

		if ( Input.Pressed( InputButton.View ) )
		{
			if ( Camera is not FirstPersonCamera )
			{
				Camera = new FirstPersonCamera();
			}
			else
			{
				Camera = new ThirdPersonCamera();
			}
		}

		if ( Input.Pressed( InputButton.Drop ) )
		{
			var dropped = Inventory.DropActive();
			if ( dropped != null )
			{
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRot.Forward * 500.0f + Vector3.Up * 100.0f, true );
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100.0f, true );

				timeSinceDropped = 0;
			}
		}

		if ( Input.Released( InputButton.Jump ) )
		{
			if ( timeSinceJumpReleased < 0.3f )
			{
				(GameBase.Current as Game)?.DoPlayerNoclip( this );
			}

			timeSinceJumpReleased = 0;
		}

		if ( Input.Left != 0 || Input.Forward != 0 )
		{
			timeSinceJumpReleased = 1;
		}
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 ) return;

		base.StartTouch( other );
	}

	[ServerCmd( "inventory_current" )]
	public static void SetInventoryCurrent( string entName )
	{
		var target = ConsoleSystem.Caller;
		if ( target == null ) return;

		var inventory = target.Inventory;
		if ( inventory == null )
			return;

		for ( int i = 0; i < inventory.Count(); ++i )
		{
			var slot = inventory.GetSlot( i );
			if ( !slot.IsValid() )
				continue;

			if ( !slot.ClassInfo.IsNamed( entName ) )
				continue;

			inventory.SetActiveSlot( i, false );

			break;
		}
	}

	public override bool HasPermission( string mode )
	{
		if ( mode == "noclip" ) return true;
		if ( mode == "devcam" ) return true;
		if ( mode == "suicide" ) return true;

		return base.HasPermission( mode );
	}
}
