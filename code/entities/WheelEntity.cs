using Sandbox;
using Sandbox.Joints;

[Library( "ent_wheel" )]
public partial class WheelEntity : Prop
{
	public RevoluteJoint Joint;

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Joint.IsValid() )
		{
			Joint.Remove();
		}
	}
}
