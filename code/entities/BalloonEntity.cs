using Sandbox;

[ClassLibrary( "ent_balloon", Title = "Balloon", Spawnable = true )]
public partial class BalloonEntity : Prop
{
	static SoundEvent PopSound = new( "sounds/balloon_pop_cute.vsnd" )
	{ 
		Volume = 1,
		DistanceMax = 500.0f
	};

	public PhysicsJoint AttachJoint;
	public Particles AttachRope;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/citizen_props/balloonregular01.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
		PhysicsBody.GravityScale = -0.2f;
		RenderColor = Color.Random.ToColor32();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( AttachJoint.IsValid() )
		{
			AttachJoint.Remove();
		}

		if ( AttachRope != null )
		{
			AttachRope.Destory( true );
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();

		PlaySound( PopSound.Name );
	}
}
