using Sandbox;

[Library]
public class DrivingController : PlayerController
{
	public CarEntity Car { get; set; }

	public override BBox GetHull() => default;

	public override void Tick()
	{
		if ( !Car.IsValid() )
			return;

		var player = Player as SandboxPlayer;
		if ( !player.IsValid() )
			return;

		if ( player.Input.Pressed( InputButton.Use ) || player.Health == 0 )
		{
			Velocity = Vector3.Zero;
			WishVelocity = Vector3.Zero;
			BaseVelocity = Vector3.Zero;
			Pos = player.WorldPos + player.WorldRot.Right * 75;
			Rot = Rotation.Identity;

			player.Parent = null;
			player.WorldPos = Pos;
			player.WorldRot = Rot;
			player.VehicleController = null;
			player.VehicleCamera = null;
			player.VehicleAnimator = null;

			Car.Driver = null;
			Car.ResetInput();
			Car = null;

			return;
		}

		player.Parent = Car;
		player.WorldPos = Vector3.Zero;
		player.WorldRot = Rotation.Identity;

		if ( Car.Driver == player )
		{
			Car.OnPlayerControlTick( player );
		}

		Pos = new Vector3( 15, -15, 20 );
		Rot = Rotation.Identity;
		ViewOffset = Vector3.Up * 40;
	}

	public override void BuildInput( ClientInput input )
	{
		input.ViewAngles.pitch = input.ViewAngles.pitch.Clamp( -90, 90 );
		input.ViewAngles.yaw = input.ViewAngles.yaw.Clamp( -89, 89 );
	}
}
