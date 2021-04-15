using Sandbox;
using System;

public class CarCamera : BaseCamera
{
	protected virtual float MinFov => 90.0f;
	protected virtual float MaxFov => 100.0f;
	protected virtual float MaxFovSpeed => 1000.0f;
	protected virtual float FovSmoothingSpeed => 4.0f;
	protected virtual float OrbitCooldown => 0.6f;
	protected virtual float OrbitSmoothingSpeed => 25.0f;
	protected virtual float OrbitReturnSmoothingSpeed => 3.0f;
	protected virtual float MinOrbitPitch => -25.0f;
	protected virtual float MaxOrbitPitch => 70.0f;
	protected virtual float FixedOrbitPitch => 10.0f;
	protected virtual float OrbitHeight => 60.0f;
	protected virtual float OrbitDistance => 140.0f;
	protected virtual float MaxOrbitReturnSpeed => 100.0f;

	private bool orbitEnabled;
	private float orbitTimer;
	private Angles orbitAngles;
	private Rotation orbitYawRot;
	private Rotation orbitPitchRot;
	private float currentFov;

	public override void Activated()
	{
		var player = Player.Local;
		if ( player == null ) return;

		orbitEnabled = false;
		orbitTimer = 0.0f;
		orbitAngles = Angles.Zero;
		orbitYawRot = Rotation.Identity;
		orbitPitchRot = Rotation.Identity;
		currentFov = MinFov;
	}

	public override void Update()
	{
		var player = Player.Local;
		if ( !player.IsValid() ) return;

		Viewer = null;

		if ( orbitEnabled && Time.Now > orbitTimer )
		{
			orbitEnabled = false;
		}

		var car = player.Parent as CarEntity;
		if ( !car.IsValid() ) return;

		var speed = car.IsValid() ? car.MovementSpeed : 0;
		var speedAbs = Math.Abs( speed );

		var carPos = car.WorldPos;
		var carRot = car.WorldRot;

		if ( orbitEnabled )
		{
			var slerpAmount = Time.Delta * OrbitSmoothingSpeed;

			orbitYawRot = Rotation.Slerp( orbitYawRot, Rotation.From( 0.0f, orbitAngles.yaw, 0.0f ), slerpAmount );
			orbitPitchRot = Rotation.Slerp( orbitPitchRot, Rotation.From( orbitAngles.pitch, 0.0f, 0.0f ), slerpAmount );
		}
		else
		{
			var targetPitch = FixedOrbitPitch.Clamp( MinOrbitPitch, MaxOrbitPitch );
			var targetYaw = speed < 0.0f ? carRot.Yaw() + 180.0f : carRot.Yaw();

			var slerpAmount = MaxOrbitReturnSpeed > 0.0f ? Time.Delta * (speedAbs / MaxOrbitReturnSpeed).Clamp( 0.0f, OrbitReturnSmoothingSpeed ) : 1.0f;

			orbitYawRot = Rotation.Slerp( orbitYawRot, Rotation.From( 0.0f, targetYaw, 0.0f ), slerpAmount );
			orbitPitchRot = Rotation.Slerp( orbitPitchRot, Rotation.From( targetPitch, 0.0f, 0.0f ), slerpAmount );

			orbitAngles.pitch = orbitPitchRot.Pitch();
			orbitAngles.yaw = orbitYawRot.Yaw();
			orbitAngles = orbitAngles.Normal;
		}

		Rot = orbitYawRot * orbitPitchRot;

		var startPos = carPos + carRot.Up * OrbitHeight;
		var targetPos = startPos + Rot.Backward * OrbitDistance;

		var tr = Trace.Ray( startPos, targetPos )
			.Ignore( player )
			.Radius( 8.0f )
			.Run();

		Pos = tr.EndPos;

		currentFov = MaxFovSpeed > 0.0f ? currentFov.LerpTo( MinFov.LerpTo( MaxFov, speedAbs / MaxFovSpeed ), Time.Delta * FovSmoothingSpeed ) : MaxFov;
		FieldOfView = currentFov;
	}

	public override void BuildInput( ClientInput input )
	{
		base.BuildInput( input );

		var player = Player.Local;
		if ( player == null ) return;

		if ( (Math.Abs( input.AnalogLook.pitch ) + Math.Abs( input.AnalogLook.yaw )) > 0.0f )
		{
			if ( !orbitEnabled )
			{
				orbitAngles = Rot.Angles();
				orbitAngles = orbitAngles.Normal;

				orbitYawRot = Rotation.From( 0.0f, orbitAngles.yaw, 0.0f );
				orbitPitchRot = Rotation.From( orbitAngles.pitch, 0.0f, 0.0f );
			}

			orbitEnabled = true;
			orbitTimer = Time.Now + OrbitCooldown;

			orbitAngles.yaw += input.AnalogLook.yaw;
			orbitAngles.pitch += input.AnalogLook.pitch;
			orbitAngles = orbitAngles.Normal;
			orbitAngles.pitch = orbitAngles.pitch.Clamp( MinOrbitPitch, MaxOrbitPitch );
		}

		input.ViewAngles = orbitAngles.WithYaw( orbitAngles.yaw - player.WorldRot.Yaw() );
		input.ViewAngles = input.ViewAngles.Normal;
	}
}

