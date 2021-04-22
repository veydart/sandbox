using Sandbox;
using Sandbox.Joints;
using System;
using System.Linq;

[Library( "gravgun" )]
public partial class GravGun : BaseCarriable, IPlayerControllable
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	protected PhysicsBody holdBody;
	protected WeldJoint holdJoint;

	protected PhysicsBody heldBody;
	protected Rotation heldRot;

	protected virtual float MaxPullDistance => 2000.0f;
	protected virtual float MaxPushDistance => 500.0f;
	protected virtual float LinearFrequency => 20.0f;
	protected virtual float LinearDampingRatio => 1.0f;
	protected virtual float AngularFrequency => 20.0f;
	protected virtual float AngularDampingRatio => 1.0f;
	protected virtual float PullForce => 20.0f;
	protected virtual float PushForce => 1000.0f;
	protected virtual float ThrowForce => 2500.0f;
	protected virtual float HoldDistance => 100.0f;
	protected virtual float AttachDistance => 250.0f;
	protected virtual float DropCooldown => 0.5f;

	private TimeSince timeSinceDrop;

	public PhysicsBody HeldBody => heldBody;

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public void OnPlayerControlTick( Player owner )
	{
		if ( owner == null )
			return;

		if ( !IsServer )
			return;

		var input = owner.Input;
		var eyePos = owner.EyePos;
		var eyeRot = owner.EyeRot;
		var eyeDir = owner.EyeRot.Forward;

		if ( heldBody.IsValid() )
		{
			if ( input.Pressed( InputButton.Attack1 ) )
			{
				heldBody.ApplyImpulse( eyeDir * (heldBody.Mass * ThrowForce) );
				heldBody.ApplyAngularImpulse( Vector3.Random * (heldBody.Mass * ThrowForce) );

				GrabEnd();
			}
			else if ( input.Pressed( InputButton.Attack2 ) )
			{
				timeSinceDrop = 0;

				GrabEnd();
			}
			else
			{
				GrabMove( eyePos, eyeDir, eyeRot );
			}

			return;
		}

		if ( timeSinceDrop < DropCooldown )
			return;

		var tr = Trace.Ray( eyePos, eyePos + eyeDir * MaxPullDistance )
			.UseHitboxes()
			.Ignore( owner )
			.Radius( 2.0f )
			.Run();

		if ( !tr.Hit || !tr.Body.IsValid() || tr.Entity.IsWorld )
			return;

		var body = tr.Body;

		if ( input.Pressed( InputButton.Attack1 ) )
		{
			if ( tr.Distance < MaxPushDistance && !IsBodyGrabbed( body ) )
			{
				var pushScale = 1.0f - Math.Clamp( tr.Distance / MaxPushDistance, 0.0f, 1.0f );
				body.ApplyImpulseAt( tr.EndPos, eyeDir * (body.Mass * (PushForce * pushScale)) );
			}
		}
		else if ( input.Down( InputButton.Attack2 ) )
		{
			if ( eyePos.Distance( body.Pos ) <= AttachDistance )
			{
				GrabStart( body, eyePos + eyeDir * HoldDistance, eyeRot );
			}
			else if ( !IsBodyGrabbed( body ) )
			{
				body.ApplyImpulse( eyeDir * (body.Mass * -PullForce) );
			}
		}
	}

	private void Activate()
	{
		if ( !holdBody.IsValid() )
		{
			holdBody = PhysicsWorld.AddBody();
			holdBody.BodyType = PhysicsBodyType.Keyframed;
		}
	}

	private void Deactivate()
	{
		GrabEnd();

		holdBody?.Remove();
		holdBody = null;
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		if ( IsServer )
		{
			Activate();
		}
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		if ( IsServer )
		{
			Deactivate();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( IsServer )
		{
			Deactivate();
		}
	}

	private static bool IsBodyGrabbed( PhysicsBody body )
	{
		// There for sure is a better way to deal with this
		if ( All.OfType<PhysGun>().Any( x => x?.HeldBody == body ) ) return true;
		if ( All.OfType<GravGun>().Any( x => x?.HeldBody == body ) ) return true;

		return false;
	}

	private void GrabStart( PhysicsBody body, Vector3 grabPos, Rotation grabRot )
	{
		if ( !body.IsValid() )
			return;

		if ( IsBodyGrabbed( body ) )
			return;

		GrabEnd();

		heldBody = body;
		heldRot = grabRot.Inverse * heldBody.Rot;

		holdBody.Pos = grabPos;
		holdBody.Rot = heldBody.Rot;

		heldBody.Wake();
		heldBody.EnableAutoSleeping = false;

		holdJoint = PhysicsJoint.Weld
			.From( holdBody )
			.To( heldBody, heldBody.LocalMassCenter )
			.WithLinearSpring( LinearFrequency, LinearDampingRatio, 0.0f )
			.WithAngularSpring( AngularFrequency, AngularDampingRatio, 0.0f )
			.Create();
	}

	private void GrabEnd()
	{
		if ( holdJoint.IsValid() )
		{
			holdJoint.Remove();
		}

		if ( heldBody.IsValid() )
		{
			heldBody.EnableAutoSleeping = true;
		}

		heldBody = null;
	}

	private void GrabMove( Vector3 startPos, Vector3 dir, Rotation rot )
	{
		if ( !heldBody.IsValid() )
			return;

		holdBody.Pos = startPos + dir * HoldDistance;
		holdBody.Rot = rot * heldRot;
	}
}
