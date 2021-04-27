using Sandbox;

partial class SandboxPlayer
{
	protected bool IsUseDisabled()
	{
		if ( ActiveChild is PhysGun physgun && physgun.HeldBody.IsValid() )
			return true;

		if ( ActiveChild is GravGun gravgun && gravgun.HeldBody.IsValid() )
			return true;

		return false;
	}

	protected override Entity FindUsable()
	{
		if ( IsUseDisabled() )
			return null;

		var tr = Trace.Ray( EyePos, EyePos + EyeRot.Forward * 85 )
			.Radius( 2 )
			.HitLayer( CollisionLayer.Debris )
			.Ignore( this )
			.Run();

		if ( tr.Entity == null ) return null;
		if ( tr.Entity is not IUse use ) return null;
		if ( !use.IsUsable( this ) ) return null;

		return tr.Entity;
	}

	protected override void UseFail()
	{
		if ( IsUseDisabled() )
			return;

		base.UseFail();
	}
}
