namespace Sandbox.Tools
{
	public interface IRemovable
	{
		void Remove();
	}

	[Library( "tool_remover", Title = "Remover", Group = "construction" )]
	public partial class RemoverTool : BaseTool
	{
		private Prop prop;
		private IRemovable removable;

		public override void OnPlayerControlTick()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				if ( this.prop.IsValid() )
				{
					this.prop.Delete();
					this.prop = null;

					return;
				}

				if ( this.removable != null )
				{
					this.removable.Remove();
					this.removable = null;

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

				if ( !tr.Hit || !tr.Entity.IsValid() || tr.Entity.IsWorld )
					return;

				if ( tr.Entity is IRemovable removable )
				{
					this.removable = removable;

					return;
				}

				if ( tr.Entity is Prop prop )
				{
					prop.PhysicsGroup?.Wake();
					this.prop = prop;

					return;
				}
			}
		}
	}
}
