namespace Sandbox.Tools
{
	[Library( "tool_boxgun", Title = "Box Shooter", Group = "fun" )]
	public class BoxShooter : BaseTool
	{
		TimeSince timeSinceShoot;

		public override void OnPlayerControlTick()
		{
			if ( Host.IsServer )
			{
				if ( Owner.Input.Pressed( InputButton.Attack1 ) )
				{
					ShootBox();
				}

				if ( Owner.Input.Down( InputButton.Attack2 ) && timeSinceShoot > 0.05f )
				{
					timeSinceShoot = 0;
					ShootBox();
				}
			}
		}

		void ShootBox()
		{
			var ent = new Prop
			{
				WorldPos = Owner.EyePos + Owner.EyeRot.Forward * 50,
				WorldRot = Owner.EyeRot
			};

			ent.SetModel( "models/citizen_props/crate01.vmdl" );
			ent.Velocity = Owner.EyeRot.Forward * 1000;
		}
	}

}
