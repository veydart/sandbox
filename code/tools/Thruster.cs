namespace Sandbox.Tools
{
	[Library( "tool_thruster", Title = "Thruster", Group = "construction" )]
	public partial class ThrusterTool : BaseTool
	{
		PreviewEntity previewModel;
		bool massless = true;

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, "models/thruster/thrusterprojector.vmdl" ) )
			{
				previewModel.RotationOffset = Rotation.FromAxis( Vector3.Right, -90 );
			}
		}

		public override void OnPlayerControlTick()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				var input = Owner.Input;

				if ( input.Pressed( InputButton.Attack2 ) )
				{
					massless = !massless;
				}

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

				var attached = !tr.Entity.IsWorld && tr.Body.IsValid() && tr.Body.PhysicsGroup != null && tr.Body.Entity.IsValid();

				if ( attached && tr.Entity is not Prop )
					return;

				if ( tr.Entity is ThrusterEntity )
				{
					// TODO: Set properties

					return;
				}

				var ent = new ThrusterEntity
				{
					WorldPos = tr.EndPos,
					WorldRot = Rotation.LookAt( tr.Normal, dir ) * Rotation.From( new Angles( 90, 0, 0 ) ),
					PhysicsEnabled = !attached,
					EnableSolidCollisions = !attached,
					TargetBody = attached ? tr.Body : null,
					Massless = massless
				};

				if ( attached )
				{
					ent.SetParent( tr.Body.Entity, tr.Body.PhysicsGroup.GetBodyBoneName( tr.Body ) );
				}

				ent.SetModel( "models/thruster/thrusterprojector.vmdl" );
			}
		}
	}
}
