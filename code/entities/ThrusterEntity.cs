using Sandbox;

[Library( "ent_thruster" )]
public partial class ThrusterEntity : Prop, IPhysicsUpdate, IFrameUpdate
{
	public float Force = 1000.0f;
	public bool Massless = false;
	public PhysicsBody TargetBody;

	public virtual void OnPostPhysicsStep( float dt )
	{
		if ( IsServer )
		{
			if ( TargetBody.IsValid() )
			{
				TargetBody.ApplyForceAt( WorldPos, WorldRot.Down * (Massless ? Force * TargetBody.Mass : Force) );
			}
			else if ( PhysicsBody.IsValid() )
			{
				PhysicsBody.ApplyForce( WorldRot.Down * (Massless ? Force * PhysicsBody.Mass : Force) );
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsClient )
		{
			KillEffects();
		}
	}
}
