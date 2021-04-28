using Sandbox;
using System;

[Library( "ent_bouncyball", Title = "Bouncy Ball", Spawnable = true )]
public partial class BouncyBallEntity : Prop, IUse
{
	public float MaxSpeed { get; set; } = 1000.0f;
	public float SpeedMul { get; set; } = 1.2f;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/beachball.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		var speed = eventData.PreVelocity.Length;
		var direction = Vector3.Reflect( eventData.PreVelocity.Normal, eventData.Normal.Normal ).Normal;
		Velocity = direction * MathF.Min( speed * SpeedMul, MaxSpeed );
	}

	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		if ( user is Player player )
		{
			player.Health += 10;

			Delete();
		}

		return false;
	}
}
