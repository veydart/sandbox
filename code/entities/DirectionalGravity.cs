using Sandbox;
using System.Linq;

[ClassLibrary( "directional_gravity", Title = "Directional Gravity", Spawnable = true )]
public partial class DirectionalGravity : Prop, IPhysicsUpdate
{
	public override void Spawn()
	{
		base.Spawn();

		DeleteOthers();

		SetModel( "models/arrow.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	private void DeleteOthers()
	{
		// Only allow one of these to be spawned at a time
		foreach ( var ent in All.OfType<DirectionalGravity>()
			.Where( x => x.IsValid() && x != this ) )
		{
			ent.Delete();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsServer )
		{
			PhysicsWorld.UseDefaultGravity();
			PhysicsWorld.WakeAllBodies();
		}
	}

	public void OnPostPhysicsStep( float dt )
	{
		if ( !IsServer )
			return;

		if ( !this.IsValid() )
			return;

		var gravity = WorldRot.Down * 800.0f;

		if ( gravity != PhysicsWorld.Gravity )
		{
			PhysicsWorld.Gravity = gravity;
			PhysicsWorld.WakeAllBodies();
		}
	}
}
