using Sandbox;

partial class SandboxPlayer
{
	static readonly EntityLimit RagdollLimit = new() { MaxTotal = 10 };

	[ClientRpc]
	private void BecomeRagdollOnClient()
	{
		var ent = new AnimEntity();
		ent.WorldPos = WorldPos;
		ent.WorldRot = WorldRot;
		ent.WorldScale = WorldScale;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		ent.CollisionGroup = CollisionGroup.Debris;
		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.TakeDecalsFrom( this );
		ent.SetRagdollVelocityFrom( this, 0.1f, 1, 1 );
		ent.EnableHitboxes = true;
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.BodyGroupMask = BodyGroupMask;
		ent.RenderColorAndAlpha = RenderColorAndAlpha;

		ent.SetInteractsAs( CollisionLayer.Debris );
		ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
		ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

		foreach ( var child in Children )
		{
			if ( child is ModelEntity e )
			{
				var model = e.GetModelName();
				if ( model != null && !model.Contains( "clothes" ) )
					continue;

				var clothing = new ModelEntity();
				clothing.SetModel( model );
				clothing.SetParent( ent, true );
				clothing.RenderColorAndAlpha = e.RenderColorAndAlpha;
			}
		}

		Corpse = ent;

		RagdollLimit.Watch( ent );
	}
}
