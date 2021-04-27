using Sandbox;

public class ViewModel : BaseViewModel
{
	protected float SwingInfluence => 0.05f;
	protected float ReturnSpeed => 5.0f;
	protected float MaxOffsetLength => 10.0f;
	protected float BobCycleTime => 7;
	protected Vector3 BobDirection => new Vector3( 0.0f, 1.0f, 0.5f );

	private Vector3 swingOffset;
	private float lastPitch;
	private float lastYaw;
	private float bobAnim;

	private bool activated = false;

	public override void UpdateCamera( Camera camera )
	{
		if ( !Player.Local.IsValid() )
			return;

		if ( !activated )
		{
			lastPitch = camera.Rot.Pitch();
			lastYaw = camera.Rot.Yaw();

			activated = true;
		}

		WorldPos = camera.Pos;
		WorldRot = camera.Rot;

		camera.ViewModelFieldOfView = FieldOfView;

		var newPitch = WorldRot.Pitch();
		var newYaw = WorldRot.Yaw();

		var pitchDelta = Angles.NormalizeAngle( newPitch - lastPitch );
		var yawDelta = Angles.NormalizeAngle( lastYaw - newYaw );

		var playerVelocity = Player.Local.Velocity;
		var verticalDelta = playerVelocity.z * Time.Delta;
		var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
		verticalDelta *= (1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).y ));
		pitchDelta -= verticalDelta * 1;

		var offset = CalcSwingOffset( pitchDelta, yawDelta );
		offset += CalcBobbingOffset( playerVelocity );

		WorldPos += WorldRot * offset;

		lastPitch = newPitch;
		lastYaw = newYaw;
	}

	protected Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
	{
		Vector3 swingVelocity = new Vector3( 0, yawDelta, pitchDelta );

		swingOffset -= swingOffset * ReturnSpeed * Time.Delta;
		swingOffset += (swingVelocity * SwingInfluence);

		if ( swingOffset.Length > MaxOffsetLength )
		{
			swingOffset = swingOffset.Normal * MaxOffsetLength;
		}

		return swingOffset;
	}

	protected Vector3 CalcBobbingOffset( Vector3 velocity )
	{
		bobAnim += Time.Delta * BobCycleTime;

		var twoPI = System.MathF.PI * 2.0f;

		if ( bobAnim > twoPI )
		{
			bobAnim -= twoPI;
		}

		var speed = new Vector2( velocity.x, velocity.y ).Length;
		speed = speed > 10.0 ? speed : 0.0f;
		var offset = BobDirection * (speed * 0.005f) * System.MathF.Cos( bobAnim );
		offset = offset.WithZ( -System.MathF.Abs( offset.z ) );

		return offset;
	}
}
