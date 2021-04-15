namespace Sandbox.Tools
{
	[ClassLibrary( "tool_wheel", Title = "Wheel", Group = "construction" )]
	public partial class WheelTool : BaseTool
	{
		PreviewEntity previewModel;

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, "models/citizen_props/wheel01.vmdl" ) )
			{
				previewModel.RotationOffset = Rotation.FromAxis( Vector3.Up, 90 );
			}
		}

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

				var attached = !tr.Entity.IsWorld && tr.Body.IsValid() && tr.Body.PhysicsGroup != null && tr.Body.Entity.IsValid();

				if ( attached && tr.Entity is not Prop )
					return;

				if ( tr.Entity is WheelEntity )
				{
					// TODO: Set properties

					return;
				}

				var ent = new WheelEntity
				{
					Pos = tr.EndPos,
					Rot = Rotation.LookAt( tr.Normal ) * Rotation.From( new Angles( 0, 90, 0 ) ),
				};

				ent.SetModel( "models/citizen_props/wheel01.vmdl" );

				ent.PhysicsBody.Mass = tr.Body.Mass;

				ent.Joint = PhysicsJoint.Revolute
					.From( ent.PhysicsBody )
					.To( tr.Body )
					.WithPivot( tr.EndPos )
					.WithBasis( Rotation.LookAt( tr.Normal ) * Rotation.From( new Angles( 90, 0, 0 ) ) )
					.Create();
			}
		}
	}
}
