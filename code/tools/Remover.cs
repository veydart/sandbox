namespace Sandbox.Tools
{
	[ClassLibrary( "tool_remover", Title = "Remover", Group = "construction" )]
	public partial class RemoverTool : BaseTool
	{
		private Prop target;

		public override void OnPlayerControlTick()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( target.IsValid() )
				{
					target.Delete();
					target = null;

					return;
				}

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

				if ( tr.Entity is not Prop prop )
					return;

				prop.PhysicsGroup?.Wake();

				target = prop;
			}
		}
	}
}
