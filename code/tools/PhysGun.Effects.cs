﻿using Sandbox;
using Sandbox.Component;
using System.Linq;

public partial class PhysGun
{
	Particles Beam;
	Particles EndNoHit;

	Vector3 lastBeamPos;
	ModelEntity lastGrabbedEntity;

	[Event.Frame]
	public void OnFrame()
	{
		UpdateEffects();
	}

	protected virtual void KillEffects()
	{
		Beam?.Destroy( true );
		Beam = null;
		BeamActive = false;

		EndNoHit?.Destroy( false );
		EndNoHit = null;

		if ( lastGrabbedEntity.IsValid() )
		{
			foreach ( var child in lastGrabbedEntity.Children.OfType<ModelEntity>() )
			{
				if ( child is Player )
					continue;

				if ( child.Components.TryGet<Glow>( out var childglow ) )
				{
					childglow.Enabled = false;
				}
			}

			if ( lastGrabbedEntity.Components.TryGet<Glow>( out var glow ) )
			{
				glow.Enabled = false;
			}

			lastGrabbedEntity = null;
		}
	}

	protected virtual void UpdateEffects()
	{
		var owner = Owner as Player;

		if ( owner == null || !BeamActive || owner.ActiveChild != this )
		{
			KillEffects();
			return;
		}

		var startPos = owner.EyePosition;
		var dir = owner.EyeRotation.Forward;

		var tr = Trace.Ray( startPos, startPos + dir * MaxTargetDistance )
			.UseHitboxes()
			.Ignore( owner, false )
			.WithAllTags( "solid" )
			.Run();

		if ( Beam == null )
		{
			Beam = Particles.Create( "particles/physgun_beam.vpcf", tr.EndPosition );
		}

		Beam.SetEntityAttachment( 0, EffectEntity, "muzzle", true );

		if ( GrabbedEntity.IsValid() && !GrabbedEntity.IsWorld )
		{
			var physGroup = GrabbedEntity.PhysicsGroup;

			if ( physGroup != null && GrabbedBone >= 0 )
			{
				var physBody = physGroup.GetBody( GrabbedBone );
				if ( physBody != null )
				{
					Beam.SetPosition( 1, physBody.Transform.PointToWorld( GrabbedPos ) );
				}
			}
			else
			{
				Beam.SetEntity( 1, GrabbedEntity, GrabbedPos, true );
			}

			lastBeamPos = GrabbedEntity.Position + GrabbedEntity.Rotation * GrabbedPos;

			EndNoHit?.Destroy( false );
			EndNoHit = null;

			if ( GrabbedEntity is ModelEntity modelEnt )
			{
				lastGrabbedEntity = modelEnt;

				var glow = modelEnt.Components.GetOrCreate<Glow>();
				glow.Enabled = true;
				glow.Width = 0.25f;
				glow.Color = new Color( 4f, 50.0f, 70.0f, 1.0f );
				glow.ObscuredColor = new Color( 4f, 50.0f, 70.0f, 0.0005f );

				foreach ( var child in lastGrabbedEntity.Children.OfType<ModelEntity>() )
				{
					if ( child is Player )
						continue;

					glow = child.Components.GetOrCreate<Glow>();
					glow.Enabled = true;
					glow.Color = new Color( 0.1f, 1.0f, 1.0f, 1.0f );
				}
			}
		}
		else
		{
			lastBeamPos = tr.EndPosition;// Vector3.Lerp( lastBeamPos, tr.EndPosition, Time.Delta * 10 );
			Beam.SetPosition( 1, lastBeamPos );

			if ( EndNoHit == null )
				EndNoHit = Particles.Create( "particles/physgun_end_nohit.vpcf", lastBeamPos );

			EndNoHit.SetPosition( 0, lastBeamPos );
		}
	}
}
