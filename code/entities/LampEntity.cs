using Sandbox;
using Sandbox.Tools;

[ClassLibrary( "ent_lamp" )]
public partial class LampEntity : SpotLightEntity, IUse, IRemovable
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
