namespace Sandbox.Tools
{
	[ClassLibrary( "tool_lamp", Title = "Lamp", Group = "Lighting" )]
	public partial class LampTool : BaseTool
	{
		PreviewEntity previewModel;

		private string Model { get; set; } = "models/torch/torch.vmdl";

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

				if ( tr.Entity is SpotLightEntity )
				{
					// TODO: Set properties

					return;
				}

				var light = new SpotLightEntity
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
					Rot = Rotation.Identity
				};

				light.SetModel( Model );
				light.SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );
				light.Pos = tr.EndPos + -light.CollisionBounds.Center + tr.Normal * light.CollisionBounds.Size * 0.5f;
			}
		}
	}
}
