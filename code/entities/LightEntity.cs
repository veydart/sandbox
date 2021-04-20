using Sandbox;
using Sandbox.Tools;

[Library( "ent_light" )]
public partial class LightEntity : PointLightEntity, IUse, IRemovable
{
	public bool IsUsable( Entity user )
	{
		return true;
	}

	public bool OnUse( Entity user )
	{
		Enabled = !Enabled;

		return false;
	}

	public void Remove()
	{
		PhysicsGroup?.Wake();
		Delete();
	}
}
