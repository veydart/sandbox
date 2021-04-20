namespace Sandbox.Tools
{
	[Library( "tool_weld", Title = "Weld", Group = "construction" )]
	public partial class WeldTool : BaseTool
	{
		private Prop target;

		public override void OnPlayerControlTick()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				var input = Owner.Input;

				if ( !input.Pressed( InputButton.Attack1 ) )
					return;

				var startPos = Owner.EyePos;
				var dir = Owner.EyeRot.Forward;

				var tr = Trace.Ray( startPos, startPos + dir * MaxTraceDistance )
					.Ignore( Owner )
					.Run();

				if ( !tr.Hit )
					return;

				if ( !tr.Entity.IsValid() )
					return;

				if ( tr.Entity.IsWorld )
					return;

				if ( tr.Entity == target )
					return;

				if ( !tr.Body.IsValid() )
					return;

				if ( tr.Entity.PhysicsGroup == null || tr.Entity.PhysicsGroup.BodyCount > 1 )
					return;

				if ( tr.Entity is not Prop prop )
					return;

				if ( !target.IsValid() )
				{
					target = prop;
				}
				else
				{
					target.Weld( prop );
					target = null;
				}
			}
		}
	}
}

//[Library( "tool_welder" )]
//public class Welder : BaseCarriable, IPlayerControllable
//{
//	static SoundEvent WeldSound = new( "sounds/tools/sfm/beep.vsnd" ) { Volume = 1 };

//	private Prop _targetProp;

//	public void OnPlayerControlTick( Player owner )
//	{
//		if ( !IsServer ) return;

//		if ( owner == null || !IsActiveChild() ) return;

//		var input = owner.Input;
//		var startPos = owner.EyePos;
//		var dir = owner.EyeRot.Forward;

//		var tr = Trace.Ray(startPos, startPos + dir * 10000.0f)
//			.Ignore(owner)
//			.Run();

//		if ( !tr.Hit || tr.Body == null || tr.Entity == null || tr.Entity.IsWorld )
//		{
//			return;
//		}

//		if ( tr.Entity == _targetProp )
//		{
//			return;
//		}

//		if ( tr.Entity.PhysicsGroup.BodyCount > 1 )
//		{
//			return;
//		}

//		if ( input.Pressed( InputButton.Attack1 ))
//		{
//			if ( tr.Entity.Root is not Prop prop )
//			{
//				return;
//			}

//			if ( !_targetProp.IsValid() )
//			{
//				_targetProp = prop;
//			}
//			else
//			{
//				_targetProp.Weld( prop );
//				_targetProp = null;
//			}

//			PlaySound( WeldSound.Name );
//		}
//		else if ( input.Pressed( InputButton.Attack2 ) )
//		{
//			if ( tr.Entity is not Prop prop )
//			{
//				return;
//			}

//			prop.Unweld( true );

//			PlaySound( WeldSound.Name );
//		}
//		else if ( input.Pressed( InputButton.Reload ) )
//		{
//			if ( tr.Entity.Root is not Prop prop )
//			{
//				return;
//			}

//			prop.Unweld();

//			PlaySound( WeldSound.Name );
//		}
//	}

//	private void Activate()
//	{
//		_targetProp = null;
//	}

//	private void Deactivate()
//	{
//		_targetProp = null;
//	}

//	public override void ActiveStart( Entity ent )
//	{
//		base.ActiveStart( ent );

//		if ( !IsServer ) return;

//		Activate();
//	}

//	public override void ActiveEnd( Entity ent, bool dropped )
//	{
//		base.ActiveEnd( ent, dropped );

//		if ( !IsServer ) return;

//		Deactivate();
//	}

//	protected override void OnDestroy()
//	{
//		base.OnDestroy();

//		if ( !IsServer ) return;

//		Deactivate();
//	}
//}
