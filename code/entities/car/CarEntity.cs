using Sandbox;
using System;

public static class CarUtil
{
	/// <summary>
	/// Turns a linear 0-1 into a more curved range, with a steeper climb. Result is mirrored when value is negative.
	/// </summary>
	/// <param name="value">The value to curve.</param>
	/// <param name="power">The amount of curve, must be greater than 1.</param>
	/// <returns></returns>
	public static float Curve( float value, float power ) =>
		(MathF.Pow( power, MathF.Abs( value ) ) - 1) / (power - 1);

	public static Vector3 Pow( Vector3 value, float p ) =>
		new( MathF.Pow( value.x, p ), MathF.Pow( value.y, p ), MathF.Pow( value.z, p ) );
}

public struct CarWheel
{
	public float distance;
}

class CarSuspension
{
	private readonly CarEntity _parent;

	private float _previousLength;
	private float _currentLength;

	public CarSuspension( CarEntity parent )
	{
		_parent = parent;
	}

	public bool Raycast( float length, bool doPhysics, Vector3 offset, ref CarWheel wheel, float dt )
	{
		var position = _parent.Pos;
		var rotation = _parent.Rot;

		var wheelAttachPos = (position + rotation.Up * 20.0f) + offset;
		var wheelExtend = wheelAttachPos - rotation.Up * length;

		var tr = Trace.Ray( wheelAttachPos, wheelExtend )
			.Ignore( _parent )
			.Run();

		if ( !doPhysics )
		{
			var wheelPosition = tr.Hit ? tr.EndPos : wheelExtend;
			wheelPosition += rotation.Up * _parent.WheelRadius;
			wheel.distance = length * tr.Fraction;

			if ( tr.Hit )
			{
				DebugOverlay.Circle( wheelPosition, rotation * Rotation.FromYaw( 90 ), _parent.WheelRadius, Color.Red.WithAlpha( 0.1f ), false );
				DebugOverlay.Line( tr.StartPos, tr.EndPos, Color.Red, 0, false );
			}
			else
			{
				DebugOverlay.Circle( wheelPosition, rotation * Rotation.FromYaw( 90 ), _parent.WheelRadius, Color.Green.WithAlpha( 0.1f ), false );
				DebugOverlay.Line( wheelAttachPos, wheelExtend, Color.Green, 0, false );
			}

			return tr.Hit;
		}

		if ( !tr.Hit )
		{
			return false;
		}

		var body = _parent.PhysicsBody;

		_previousLength = _currentLength;
		_currentLength = length - tr.Distance;

		var springVelocity = (_currentLength - _previousLength) / dt;
		var springForce = body.Mass * 50.0f * _currentLength;
		var damperForce = body.Mass * (1.5f + (1.0f - tr.Fraction) * 3.0f) * springVelocity;
		var velocity = body.GetVelocityAtPoint( wheelAttachPos );
		var speed = velocity.Length;
		var speedDot = MathF.Abs( speed ) > 0.0f ? MathF.Abs( MathF.Min( Vector3.Dot( velocity, rotation.Up.Normal ) / speed, 0.0f ) ) : 0.0f;
		var speedAlongNormal = speedDot * speed;
		var correctionMultiplier = (1.0f - tr.Fraction) * (speedAlongNormal / 1000.0f);
		var correctionForce = correctionMultiplier * 50.0f * speedAlongNormal / dt;

		body.ApplyImpulseAt( wheelAttachPos, tr.Normal * (springForce + damperForce + correctionForce) * dt );

		return true;
	}
}

[Library( "ent_car", Title = "Car", Spawnable = true )]
public partial class CarEntity : Prop, IPhysicsUpdate, IFrameUpdate
{
	private bool _turnWheelsOnGround;
	private bool _driveWheelsOnGround;
	private float _accelerateDirection;
	private float _wheelAngle;

	private CarSuspension _frontLeft, _frontRight, _backLeft, _backRight;
	private CarWheel _wheelFrontLeft, _wheelFrontRight, _wheelBackLeft, _wheelBackRight;

	public float WheelRadius => 14.0f;

	[Net]
	private float _wheelSpeed { get; set; }

	[Net]
	private float _turnDirection { get; set; }

	[Net]
	private float _accelerationTilt { get; set; }

	[Net]
	private float _turnLean { get; set; }

	[Net]
	public float MovementSpeed { get; set; }

	private struct InputState
	{
		public float throttle;
		public float turning;
		public float breaking;

		public void Reset()
		{
			throttle = 0;
			turning = 0;
			breaking = 0;
		}
	}

	private InputState currentInput;

	bool spawned = false;

	public CarEntity()
	{
		_frontLeft = new CarSuspension( this );
		_frontRight = new CarSuspension( this );
		_backLeft = new CarSuspension( this );
		_backRight = new CarSuspension( this );
	}

	private Entity chassis_axle_rear;
	private Entity chassis_axle_front;
	private Entity wheel0;
	private Entity wheel1;
	private Entity wheel2;
	private Entity wheel3;

	public Player Driver { get; set; }

	public void Drive( Player owner )
	{
		if ( Driver.IsValid() )
			return;

		var player = owner as SandboxPlayer;
		if ( !player.IsValid() )
			return;

		if ( player.VehicleController != null )
			return;

		player.VehicleController = new DrivingController { Car = this };
		player.VehicleCamera = new CarCamera();
		player.VehicleAnimator = new CarAnimator();
		player.ResetAnimParams();

		Driver = player;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "entities/modular_vehicle/chassis_2_main.vmdl" );
		SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

		{
			var w = new ModelEntity();
			w.SetModel( "entities/modular_vehicle/vehicle_fuel_tank.vmdl" );
			w.Parent = this;
			w.Pos = new Vector3( 0.75f, 0, 0 ) * 40.0f;
		}

		{
			var w = new ModelEntity();
			w.SetModel( "entities/modular_vehicle/chassis_axle_front.vmdl" );
			w.Parent = this;
			w.Pos = new Vector3( 1.05f, 0, 0.35f ) * 40.0f;

			chassis_axle_front = w;

			{
				var w2 = new ModelEntity();
				w2.SetModel( "entities/modular_vehicle/wheel_a.vmdl" );
				w2.SetParent( w, "Wheel_Steer_R", new Transform( Vector3.Zero, Rotation.From( 0, 180, 0 ) ) );
				wheel0 = w2;
			}

			{
				var w2 = new ModelEntity();
				w2.SetModel( "entities/modular_vehicle/wheel_a.vmdl" );
				w2.SetParent( w, "Wheel_Steer_L", new Transform( Vector3.Zero, Rotation.From( 0, 0, 0 ) ) );
				wheel1 = w2;
			}

			{
				var w2 = new ModelEntity();
				w2.SetModel( "entities/modular_vehicle/chassis_steering.vmdl" );
				w2.SetParent( w, "Axle_front_Center", new Transform( Vector3.Zero, Rotation.From( -90, 180, 0 ) ) );
			}
		}

		{
			var w = new ModelEntity();
			w.SetModel( "entities/modular_vehicle/chassis_axle_rear.vmdl" );
			w.Parent = this;
			w.Pos = new Vector3( -1.05f, 0, 0.35f ) * 40.0f;

			chassis_axle_rear = w;

			{
				var w2 = new ModelEntity();
				w2.SetModel( "entities/modular_vehicle/chassis_transmission.vmdl" );
				w2.Parent = this;
				w2.SetParent( w, "Axle_Rear_Center", new Transform( Vector3.Zero, Rotation.From( -90, 180, 0 ) ) );
			}

			{
				var w2 = new ModelEntity();
				w2.SetModel( "entities/modular_vehicle/wheel_a.vmdl" );
				w2.SetParent( w, "Axle_Rear_Center", new Transform( Vector3.Left * (0.7f * 40), Rotation.From( 0, 90, 0 ) ) );
				wheel2 = w2;
			}

			{
				var w2 = new ModelEntity();
				w2.SetModel( "entities/modular_vehicle/wheel_a.vmdl" );
				w2.SetParent( w, "Axle_Rear_Center", new Transform( Vector3.Right * (0.7f * 40), Rotation.From( 0, -90, 0 ) ) );
				wheel3 = w2;
			}
		}

		spawned = true;
	}

	public void ResetInput()
	{
		currentInput.Reset();
	}

	public void OnPlayerControlTick( Player owner )
	{
		if ( owner == null ) return;
		if ( !IsServer ) return;

		if ( !owner.IsValid() ) return;
		if ( Driver != owner ) return;

		using ( Prediction.Off() )
		{
			var input = owner.Input;

			currentInput.Reset();
			currentInput.throttle = (input.Down( InputButton.Forward ) ? 1 : 0) + (input.Down( InputButton.Back ) ? -1 : 0);
			currentInput.turning = (input.Down( InputButton.Left ) ? 1 : 0) + (input.Down( InputButton.Right ) ? -1 : 0);
			currentInput.breaking = (input.Down( InputButton.Jump ) ? 1 : 0);

			if ( input.Pressed( InputButton.Reload ) )
			{
				Pos = new Vector3( 1700, -1000, 100 );
				Rot = Rotation.Identity;
				PhysicsBody.Velocity = 0;
				PhysicsBody.AngularVelocity = 0;
			}
		}
	}

	public void OnPostPhysicsStep( float dt )
	{
		if ( !this.IsValid() )
			return;

		if ( !spawned )
			return;

		if ( !IsServer )
			return;

		var body = PhysicsBody;

		body.LinearDrag = 0.0f;
		body.AngularDrag = 0.0f;
		body.LinearDamping = 0;
		body.AngularDamping = 0;
		body.GravityScale = (_driveWheelsOnGround || _turnWheelsOnGround) ? 0 : 1;

		var rotation = body.Rot;

		_accelerateDirection = currentInput.throttle.Clamp( -1, 1 );
		_turnDirection = _turnDirection.LerpTo( currentInput.turning.Clamp( -1, 1 ), 1.0f - MathF.Pow( 0.0075f, Time.Delta ) );

		float targetTilt = 0;
		float targetLean = 0;

		var speed = rotation.Inverse * body.Velocity;

		if ( _driveWheelsOnGround || _turnWheelsOnGround )
		{
			var forwardSpeed = MathF.Abs( speed.x );
			var speedFraction = MathF.Min( forwardSpeed / 500.0f, 1 );

			targetTilt = (_accelerateDirection).Clamp( -1.0f, 1.0f );
			targetLean = speedFraction * _turnDirection;
		}

		_accelerationTilt = _accelerationTilt.LerpTo( targetTilt, 1.0f - MathF.Pow( 0.01f, dt ) );
		_turnLean = _turnLean.LerpTo( targetLean, 1.0f - MathF.Pow( 0.01f, dt ) );

		if ( _driveWheelsOnGround )
		{
			var forwardSpeed = MathF.Abs( speed.x );
			var speedMultiplier = 1.0f - (forwardSpeed / 5000.0f).Clamp( 0.0f, 1.0f );
			var acceleration = speedMultiplier * (_accelerateDirection < 0.0f ? 200.0f : 500.0f) * body.Mass * (_driveWheelsOnGround ? _accelerateDirection : 1.0f) * dt;
			var impulseLocation = body.Pos + rotation * new Vector3( MathF.Sign( acceleration ) * 100.0f, 0, 0 );
			var impulse = rotation * new Vector3( acceleration, 0, 0 );

			body.ApplyImpulseAt( impulseLocation + rotation.Up * 25, impulse );
		}

		speed = rotation.Inverse * body.Velocity;
		MovementSpeed = speed.x;

		RaycastWheels( rotation, true, out var frontWheels, out var backWheels, dt );

		var onGround = frontWheels || backWheels;
		_turnWheelsOnGround = frontWheels;
		_driveWheelsOnGround = backWheels;

		if ( onGround )
		{
			_wheelSpeed = speed.x;

			var grip = 1.0f - CarUtil.Curve( 1.0f - Vector3.Dot( rotation.Up, Vector3.Up ), 1000.0f );

			body.Velocity += (Vector3.Down * 900.0f) * dt;

			{
				var dampening = new Vector3( 0.2f, grip, 0.0f );
				var dampened = (rotation.Inverse * body.Velocity) * CarUtil.Pow( 1.0f - dampening, dt );
				body.Velocity = (rotation * dampened);
			}

			{
				// angular dampen
				var unrotatedAngular = rotation.Inverse * body.AngularVelocity;
				var angularDampening = new Vector3( 0, 0, 0.999f );
				var dampenedAngular = unrotatedAngular * CarUtil.Pow( 1.0f - angularDampening, dt );
				body.AngularVelocity = rotation * dampenedAngular;
			}

			var localVelocity = rotation.Inverse * body.Velocity;
			var turnAmount = _turnWheelsOnGround ? (MathF.Sign( localVelocity.x ) * 100.0f * CalculateTurnFactor( MathF.Abs( localVelocity.x ) ) * dt) : 0.0f;

			// turning
			if ( MathF.Abs( turnAmount ) > 0.0f )
			{
				body.AngularVelocity += rotation * new Vector3( 0, 0, turnAmount );
			}

			{
				var dampening = new Vector3( 0.2f, grip, 0.0f );
				var dampened = (rotation.Inverse * body.Velocity) * CarUtil.Pow( 1.0f - dampening, dt );
				body.Velocity = (rotation * dampened);
			}

			{
				// angular dampen
				var unrotatedAngular = rotation.Inverse * body.AngularVelocity;
				var angularDampening = new Vector3( 0, 0, 0.999f );
				var dampenedAngular = unrotatedAngular * CarUtil.Pow( 1.0f - angularDampening, dt );
				body.AngularVelocity = rotation * dampenedAngular;
			}
		}
	}

	private void RaycastWheels( Rotation rotation, bool doPhysics, out bool frontWheels, out bool backWheels, float dt )
	{
		float f = 42;
		float r = 32;

		var frontLeftPos = rotation.Forward * f + rotation.Right * r;
		var frontRightPos = rotation.Forward * f - rotation.Right * r;
		var backLeftPos = -rotation.Forward * f + rotation.Right * r;
		var backRightPos = -rotation.Forward * f - rotation.Right * r;

		var tiltAmount = _accelerationTilt * 2.5f;
		var leanAmount = _turnLean * 2.5f;

		var length = 20.0f;

		frontWheels =
			_frontLeft.Raycast( length + tiltAmount - leanAmount, doPhysics, frontLeftPos, ref _wheelFrontLeft, dt ) |
			_frontRight.Raycast( length + tiltAmount + leanAmount, doPhysics, frontRightPos, ref _wheelFrontRight, dt );

		backWheels =
			_backLeft.Raycast( length - tiltAmount - leanAmount, doPhysics, backLeftPos, ref _wheelBackLeft, dt ) |
			_backRight.Raycast( length - tiltAmount + leanAmount, doPhysics, backRightPos, ref _wheelBackRight, dt );
	}

	private float CalculateTurnFactor( float forwardSpeed )
	{
		var turnFactor = MathF.Min( forwardSpeed / 1500.0f, 1 );
		var yawSpeedFactor = 1.0f - (forwardSpeed / 1500.0f).Clamp( 0, 0.5f );

		return _turnDirection * turnFactor * yawSpeedFactor;
	}

	private float _wheelRot;

	public void OnFrame()
	{
		// ALL THIS SHIT SHOULD BE HANDLED IN AN ANIM GRAPH

		//_wheelAngle = _wheelAngle.LerpTo( CalculateTurnFactor( Math.Abs( _wheelSpeed ) ), 1.0f - MathF.Pow( 0.01f, Time.Delta ) );
		//_wheelRot += (_wheelSpeed / WheelRadius).RadianToDegree() * Time.Delta;

		//var wheelRotRight = Rotation.From( -_wheelAngle * 180, 180, -_wheelRot );
		//var wheelRotLeft = Rotation.From( _wheelAngle * 180, 0, _wheelRot );
		//var wheelRotBackRight = Rotation.From( 0, 90, -_wheelRot );
		//var wheelRotBackLeft = Rotation.From( 0, -90, _wheelRot );

		//RaycastWheels( Rot, false, out _, out _, Time.Delta );

		//if ( chassis_axle_rear.IsValid() )
		//{
		//	var e = chassis_axle_rear as ModelEntity;
		//	for ( int i = 0; i < e.BoneCount; ++i )
		//	{
		//		var boneName = e.GetBoneName( i );

		//		if ( boneName == "Axle_Rear_Center" )
		//		{
		//			float d = 20.0f - Math.Min( _wheelBackLeft.distance, _wheelBackRight.distance );
		//			e.SetBoneTransform( i, new Transform( Vector3.Up * d ), false );
		//		}
		//	}
		//}

		//if ( chassis_axle_front.IsValid() )
		//{
		//	var e = chassis_axle_front as ModelEntity;
		//	for ( int i = 0; i < e.BoneCount; ++i )
		//	{
		//		var boneName = e.GetBoneName( i );

		//		if ( boneName == "Axle_front_Center" )
		//		{
		//			float d = 20.0f - Math.Min( _wheelFrontLeft.distance, _wheelFrontRight.distance );
		//			e.SetBoneTransform( i, new Transform( Vector3.Up * d ), false );
		//		}
		//	}
		//}

		//if ( wheel0.IsValid() )
		//{
		//	wheel0.Rot = wheelRotRight;
		//}

		//if ( wheel1.IsValid() )
		//{
		//	wheel1.Rot = wheelRotLeft;
		//}

		//if ( wheel2.IsValid() )
		//{
		//	wheel2.Rot = wheelRotBackRight;
		//}

		//if ( wheel3.IsValid() )
		//{
		//	wheel3.Rot = wheelRotBackLeft;
		//}
	}
}
