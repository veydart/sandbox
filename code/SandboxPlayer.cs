using Sandbox;

partial class SandboxPlayer : BasePlayer
{
	TimeSince timeSinceDropped;

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

		//
		Inventory.DropActive();

		//
		// Delete any items we didn't drop
		//
		Inventory.DeleteContents();

		BecomeRagdollOnClient();

		Controller = null;
		Camera = new SpectateRagdollCamera();

		EnableAllCollisions = false;
		EnableDrawing = false;
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


	TimeSince timeSinceJumpReleased;

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
			if ( !(Camera is FirstPersonCamera) )
			{
				Camera = new FirstPersonCamera();
			}
			else
			{
				// This is here for now so I can use it for recording clips etc
				if ( Input.Down( InputButton.Use ) )
				{
					var startPos = EyePos;
					var dir = EyeRot.Forward;

					var tr = Trace.Ray( startPos, startPos + dir * 10000.0f )
						.Ignore( this )
						.Run();

					if ( tr.Hit && tr.Entity.IsValid() && !tr.Entity.IsWorld )
					{
						Camera = new LookAtCamera
						{
							Origin = startPos,
							TargetEntity = tr.Entity
						};
					}
				}
				else
				{
					Camera = new ThirdPersonCamera();
				}
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
				(Game.Current as Game)?.DoPlayerNoclip( this );
			}

			timeSinceJumpReleased = 0;
		}

		if ( Input.Left != 0 || Input.Forward != 0 )
		{
			timeSinceJumpReleased = 1;
		}
	}

	static EntityLimit RagdollLimit = new() { MaxTotal = 10 };

	[ClientRpc]
	void BecomeRagdollOnClient()
	{
		var ent = new AnimEntity();
		ent.WorldPos = WorldPos;
		ent.WorldRot = WorldRot;
		ent.WorldScale = WorldScale;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		ent.CollisionGroup = CollisionGroup.Debris;
		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.TakeDecalsFrom( this );
		ent.SetRagdollVelocityFrom( this, 0.1f, 1, 1 );
		ent.EnableHitboxes = true;
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.BodyGroupMask = BodyGroupMask;

		ent.SetInteractsAs( CollisionLayer.Debris );
		ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		foreach ( var child in Children )
		{
			if ( child is ModelEntity e )
			{
				var model = e.GetModelName();
				if ( model != null && !model.Contains( "clothes" ) )
					continue;

				var clothing = new ModelEntity();
				clothing.SetModel( model );
				clothing.SetParent( ent, true );
			}
		}

		Corpse = ent;

		RagdollLimit.Watch( ent );
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

	protected bool IsUseDisabled()
	{
		if ( ActiveChild is PhysGun physgun && physgun.HeldBody.IsValid() )
			return true;

		if ( ActiveChild is GravGun gravgun && gravgun.HeldBody.IsValid() )
			return true;

		return false;
	}

	protected override Entity FindUsable()
	{
		if ( IsUseDisabled() )
			return null;

		var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 85 )
			.Radius( 2 )
			.HitLayer( CollisionLayer.Debris )
			.Ignore( this )
			.Run();

		if ( tr.Entity == null ) return null;
		if ( tr.Entity is not IUse use ) return null;
		if ( !use.IsUsable( this ) ) return null;

		return tr.Entity;
	}

	protected override void UseFail()
	{
		if ( IsUseDisabled() )
			return;

		base.UseFail();
	}
}
