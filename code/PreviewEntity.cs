
namespace Sandbox.Tools
{
	public class PreviewEntity : ModelEntity
	{
		public bool RelativeToNormal { get; set; } = true;
		public bool OffsetBounds { get; set; } = false;
		public Rotation RotationOffset { get; set; } = Rotation.Identity;
		public Vector3 PositionOffset { get; set; } = Vector3.Zero;

		internal void UpdateFromTrace( TraceResult tr )
		{
			if ( !IsTraceValid( tr ) )
			{
				RenderAlpha = 0.0f;
				return;
			}

			if ( RelativeToNormal )
			{
				WorldRot = Rotation.LookAt( tr.Normal, tr.Direction ) * RotationOffset;
				WorldPos = tr.EndPos + WorldRot * PositionOffset;
			}
			else
			{
				WorldRot = Rotation.Identity * RotationOffset;
				WorldPos = tr.EndPos + PositionOffset;
			}

			if ( OffsetBounds )
			{
				WorldPos += tr.Normal * CollisionBounds.Size * 0.5f;
			}

			RenderAlpha = 0.5f;
		}

		protected virtual bool IsTraceValid( TraceResult tr ) => tr.Hit;

	}
}
