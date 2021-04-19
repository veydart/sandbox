using Sandbox;
using Steamworks.Data;
using System.Numerics;
using System.Collections.Generic;
using Sandbox.Tools;

[ClassLibrary( "weapon_tool" )]
partial class Tool : BaseWeapon, IFrameUpdate
{
	[UserVar( "tool_current" )]
	public static string UserToolCurrent { get; set; } = "tool_boxgun";

	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	[Net]
	public BaseTool CurrentTool { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override void OnPlayerControlTick( Player owner )
	{
		base.OnPlayerControlTick( owner );

		UpdateCurrentTool( owner );

		CurrentTool?.OnPlayerControlTick();
	}

	void UpdateCurrentTool( Player owner )
	{
		var toolName = owner.GetUserString( "tool_current", "tool_boxgun" );
		if ( toolName == null )
			return;

		DebugOverlay.ScreenText( 0, $"tool_current: {toolName}" );
		DebugOverlay.ScreenText( 1, $" CurrentTool: {CurrentTool}" );

		// Already the right tool
		if ( CurrentTool != null && CurrentTool.Parent == this && CurrentTool.Owner == owner && CurrentTool.ClassInfo.IsNamed( toolName ) )
			return;

		if ( CurrentTool != null )
		{
			CurrentTool?.Deactivate();
			CurrentTool = null;
		}

		CurrentTool = Library.Create<BaseTool>( toolName, false );

		if ( CurrentTool != null )
		{
			CurrentTool.Parent = this;
			CurrentTool.Owner = owner;
			CurrentTool.Activate();
		}
	}

	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		CurrentTool?.Activate();
	}

	public override void ActiveEnd( Entity ent, bool dropped )
	{
		base.ActiveEnd( ent, dropped );

		CurrentTool?.Deactivate();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		CurrentTool?.Deactivate();
		CurrentTool = null;
	}

	public virtual void OnFrame()
	{
		if ( !IsActiveChild() ) return;

		CurrentTool?.OnFrame();
	}
}

namespace Sandbox.Tools
{
	public partial class BaseTool : NetworkClass
	{
		public ModelEntity Parent { get; set; }
		public Player Owner { get; set; }

		protected virtual float MaxTraceDistance => 10000.0f;

		public virtual void Activate()
		{
			CreatePreviews();
		}

		public virtual void Deactivate()
		{
			DeletePreviews();
		}

		public virtual void OnPlayerControlTick()
		{

		}

		public virtual void OnFrame()
		{
			UpdatePreviews();
		}
	}
}
