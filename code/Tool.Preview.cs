using System.Collections.Generic;

namespace Sandbox.Tools
{
	public partial class BaseTool
	{
		internal List<PreviewEntity> Previews;

		public virtual void CreatePreviews()
		{
			// Nothing
		}

		public virtual void DeletePreviews()
		{
			if ( Previews == null || Previews.Count == 0 )
				return;

			foreach ( var preview in Previews )
			{
				preview.Delete();
			}

			Previews.Clear();
		}


		public virtual bool TryCreatePreview( ref PreviewEntity ent, string model )
		{
			if ( !ent.IsValid() )
			{
				ent = new PreviewEntity();
				ent.SetModel( model );
			}

			if ( Previews == null )
			{
				Previews = new List<PreviewEntity>();
			}

			if ( !Previews.Contains( ent ) )
			{
				Previews.Add( ent );
			}

			return ent.IsValid();
		}


		private void UpdatePreviews()
		{
			if ( Previews == null || Previews.Count == 0 )
				return;

			var startPos = Owner.EyePos;
			var dir = Owner.EyeRot.Forward;

			var tr = Trace.Ray( startPos, startPos + dir * 10000.0f )
				.Ignore( Owner )
				.Run();

			foreach ( var preview in Previews )
			{
				preview.UpdateFromTrace( tr );
			}
		}
	}
}
