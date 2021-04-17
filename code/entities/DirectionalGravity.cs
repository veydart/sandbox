using Sandbox;
using System.Linq;

[ClassLibrary( "directional_gravity", Title = "Directional Gravity", Spawnable = true )]
public partial class DirectionalGravity : Prop, IPhysicsUpdate
{
	public override void Spawn()
	{
		base.Spawn();

		// Only allow one of these to be spawned at a time
		foreach ( var ent in All.OfType<DirectionalGravity>()
			.Where( x => x.IsValid() && x != this ))
		{
			ent.Delete();
		}

		SetModel( "models/arrow.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsServer )
		{
			// We probably want a better way to restore previous gravity
			PhysicsWorld.Gravity = Vector3.Down * 800.0f;
		}
	}

	public void OnPostPhysicsStep( float dt )
	{
		if ( !IsServer )
			return;

		if ( !this.IsValid() )
			return;

		PhysicsWorld.Gravity = WorldRot.Down * 800.0f;
	}
}
