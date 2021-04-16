using Sandbox;

[ClassLibrary( "directional_gravity", Title = "Directional Gravity", Spawnable = true )]
public partial class DirectionalGravity : Prop, IPhysicsUpdate
{
	public override void Spawn()
	{
		base.Spawn();

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

		if ( !PhysicsBody.IsValid() )
			return;

		PhysicsWorld.Gravity = PhysicsBody.Rot.Down * 800.0f;
	}
}
