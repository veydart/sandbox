using Sandbox;

public class CarAnimator : PlayerAnimator
{
	public override void Tick()
	{
		SetLookAt( "lookat_pos", Player.EyePos + Input.Rot.Forward * 1000 );

		SetParam( "b_ducked", true );

		if ( Player.ActiveChild is BaseCarriable carry )
		{
			carry.TickPlayerAnimator( this );
		}
		else
		{
			SetParam( "holdtype", 0 );
			SetParam( "aimat_weight", 0.0f );
		}
	}
}
