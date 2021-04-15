using Sandbox;

public partial class ThrusterEntity
{
	private Particles effects;

	public virtual void OnFrame()
	{
		UpdateEffects();
	}

	protected virtual void KillEffects()
	{
		effects?.Destory(false);
		effects = null;
	}

	protected virtual void UpdateEffects()
	{
		if ( effects == null )
		{
			effects = Particles.Create( "particles/physgun_end_nohit.vpcf" );
			//effects.SetEntity( 0, this, Vector3.Up * 20, true );
		}

		effects.SetPos( 0, WorldPos + WorldRot.Up * 20 );
	}
}
