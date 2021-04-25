using Sandbox;

[Library( "flashlight", Title = "Flashlight", Spawnable = true )]
partial class Flashlight : Weapon
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override void OnPlayerControlTick( Player owner )
	{

	}
}
