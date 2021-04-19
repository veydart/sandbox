using Sandbox;
using Sandbox.Tools;

[ClassLibrary( "ent_lamp" )]
public partial class LampEntity : SpotLightEntity, IRemovable
{
	public void Remove()
	{
		PhysicsGroup?.Wake();
		Delete();
	}
}
