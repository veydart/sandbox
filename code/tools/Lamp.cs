namespace Sandbox.Tools
{
	[Library( "tool_lamp", Title = "Lamp", Group = "construction" )]
	public partial class LampTool : BaseTool
	{
		PreviewEntity previewModel;

		private string Model => "models/torch/torch.vmdl";

		protected override bool IsPreviewTraceValid( TraceResult tr )
		{
			if ( !base.IsPreviewTraceValid( tr ) )
				return false;

			if ( tr.Entity is LampEntity )
				return false;

			return true;
		}

		public override void CreatePreviews()
		{
			if ( TryCreatePreview( ref previewModel, Model ) )
			{
				previewModel.RelativeToNormal = false;
				previewModel.OffsetBounds = true;
				previewModel.PositionOffset = -previewModel.CollisionBounds.Center;
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

				if ( !tr.Hit || !tr.Entity.IsValid() )
					return;

				if ( tr.Entity is LampEntity lamp )
				{
					// TODO: Set properties

					lamp.Flicker = !lamp.Flicker;

					return;
				}

				lamp = new LampEntity
				{
					Enabled = true,
					DynamicShadows = true,
					Range = 512,
					Falloff = 1.0f,
					LinearAttenuation = 0.0f,
					QuadraticAttenuation = 1.0f,
					InnerConeAngle = 25,
					OuterConeAngle = 45,
					Brightness = 10,
					Color = Color.Random,
					WorldRot = Rotation.Identity
				};

				lamp.SetModel( Model );
				lamp.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
				lamp.WorldPos = tr.EndPos + -lamp.CollisionBounds.Center + tr.Normal * lamp.CollisionBounds.Size * 0.5f;
			}
		}
	}
}
