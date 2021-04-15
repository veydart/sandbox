namespace Sandbox.Tools
{
	[ClassLibrary( "tool_leafblower", Title = "Leaf Blower", Group = "fun" )]
	public partial class LeafBlowerTool : BaseTool
	{
		protected virtual float Force => 128;
		protected virtual float MaxDistance => 512;
		protected virtual bool Massless => true;

		public override void OnPlayerControlTick()
		{
			if ( !Host.IsServer )
				return;

			using ( Prediction.Off() )
			{
				var input = Owner.Input;

				if ( !input.Down( InputButton.Attack1 ) )
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

				var body = tr.Body;

				if ( !body.IsValid() )
					return;

				var direction = tr.EndPos - tr.StartPos;
				var distance = direction.Length;
				var ratio = ( 1.0f - ( distance / MaxDistance ) ).Clamp( 0, 1 );
				var force = direction * ( Force * ratio );

				if ( Massless )
				{
					force *= body.Mass;
				}

				body.ApplyForceAt( tr.EndPos, force );
			}
		}
	}
}
