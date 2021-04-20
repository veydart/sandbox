
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Threading.Tasks;

[Library]
public partial class SandboxHud : Hud
{
	public SandboxHud()
	{
		if ( !IsClient )
			return;

		RootPanel.StyleSheet.Load( "/ui/SandboxHud.scss" );

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<CrosshairCanvas>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Scoreboard<ScoreboardEntry>>();

		var healthPanel = RootPanel.Add.Panel( "health" ); 
		var icon = healthPanel.Add.Label( "🩸", "icon" );
		var health = healthPanel.Add.Label( "", "value" );
		health.Text = "100";

		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<SpawnMenu>();
	}
}
