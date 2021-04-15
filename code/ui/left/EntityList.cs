
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;

[ClassLibrary]
public partial class EntityList : Panel
{
	Panel Canvas;

	public EntityList()
	{
		AddClass( "spawnpage" );
		Canvas = Add.Panel( "canvas" );

		var ents = Library.GetAllAttributes<Entity>().Where( x => x.Spawnable ).OrderBy( x => x.Title ).ToArray();

		foreach ( var entry in ents )
		{
			//if ( file.Contains( "_lod0" ) ) continue;
			//if ( file.Contains( "clothes" ) ) continue;

			var btn = Canvas.Add.Button( entry.Title );
			btn.AddClass( "icon" );
			btn.AddEvent( "onclick", () => ConsoleSystem.Run( "spawn_entity", entry.Name ) );
			btn.Style.Background = new PanelBackground
			{
				Texture = Texture.Load( $"/entity/{entry.Name}.png", false )
			};
		}
	}

}
