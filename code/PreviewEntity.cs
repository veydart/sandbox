using System;

namespace Sandbox.Tools
{
	public class PreviewEntity : ModelEntity
	{
		public bool RelativeToNormal { get; set; } = true;
		public Rotation RotationOffset { get; set; } = Rotation.Identity;

		internal void UpdateFromTrace( TraceResult tr )
		{
			if ( !IsTraceValid( tr ) )
			{
				RenderAlpha = 0.0f;
				return;
			}

			WorldPos = tr.EndPos;

			if ( RelativeToNormal )
			{
				WorldRot = Rotation.LookAt( tr.Normal, tr.Direction ) * RotationOffset;
			}
			else
			{
				WorldRot = Rotation.Identity * RotationOffset;
			}

			
			RenderAlpha = 0.5f;
		}

		protected virtual bool IsTraceValid( TraceResult tr ) => tr.Hit;

	}
}
