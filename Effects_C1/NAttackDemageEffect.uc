class NAttackDemageEffect extends Emitter;

defaultproperties
{
	Begin Object Class=MeshEmitter Name=MeshEmitter1
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Impact.sparkredcone00'
		UseMeshBlendMode=false
		RenderTwoSided=true
		ColorMultiplierRange=(X=(Min=0.8,Max=0.9),Y=(Min=0.8,Max=0.9),Z=(Min=0.8,Max=0.9))
		FadeOut=true
		MaxParticles=1
		RespawnDeadParticles=false
		AutoDestroy=true
		Name="MeshEmitter1"
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=0.2)
		SizeScale(1)=(RelativeTime=0.15,RelativeSize=2.6)
		SizeScale(2)=(RelativeTime=0.5,RelativeSize=3.6)
		StartSizeRange=(X=(Min=0.2,Max=0.2),Y=(Min=0.16,Max=0.16),Z=(Min=0.16,Max=0.16))
		InitialParticlesPerSecond=1000.0
		AutomaticInitialSpawning=false
		SecondsBeforeInactive=1.0
		LifetimeRange=(Min=0.36,Max=0.36)
	End Object
	Emitters(0)=MeshEmitter'MeshEmitter1'
	Begin Object Class=SpriteEmitter Name=SpriteEmitter1
		UseDirectionAs=PTDU_Normal
		ProjectionNormal=(X=1.0,Y=0.0,Z=0.0)
		UseColorScale=true
		ColorMultiplierRange=(X=(Min=20.0,Max=20.0),Y=(Min=20.0,Max=20.0),Z=(Min=1.0,Max=1.0))
		FadeOutStartTime=0.04
		FadeOut=true
		MaxParticles=1
		RespawnDeadParticles=false
		AutoDestroy=true
		Name="SpriteEmitter1"
		StartMassRange=(Min=0.0,Max=0.0)
		StartSpinRange=(X=(Min=0.0,Max=360.0),Y=(Min=0.0,Max=360.0),Z=(Min=0.0,Max=360.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=1.0)
		SizeScale(1)=(RelativeTime=0.2,RelativeSize=8.0)
		StartSizeRange=(X=(Min=2.2,Max=2.2),Y=(Min=100.0,Max=100.0),Z=(Min=100.0,Max=100.0))
		UniformSize=true
		InitialParticlesPerSecond=1000.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_Brighten
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0006'
		TextureUSubdivisions=2
		TextureVSubdivisions=2
		SubdivisionEnd=1
		SecondsBeforeInactive=1.0
		LifetimeRange=(Min=0.2,Max=0.2)
	End Object
	Emitters(1)=SpriteEmitter'SpriteEmitter1'
	Begin Object Class=SpriteEmitter Name=SpriteEmitter2
		UseDirectionAs=PTDU_Up
		Acceleration=(X=0.0,Y=0.0,Z=-300.0)
		FadeOut=true
		RespawnDeadParticles=false
		AutoDestroy=true
		Name="SpriteEmitter2"
		StartMassRange=(Min=0.0,Max=0.0)
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=1.5)
		StartSizeRange=(X=(Min=0.6,Max=0.6),Y=(Min=3.0,Max=3.0),Z=(Min=100.0,Max=100.0))
		InitialParticlesPerSecond=3000.0
		AutomaticInitialSpawning=false
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0000'
		TextureUSubdivisions=8
		TextureVSubdivisions=8
		SubdivisionStart=11
		SubdivisionEnd=12
		SecondsBeforeInactive=1.0
		LifetimeRange=(Min=0.3,Max=0.5)
		StartVelocityRange=(X=(Min=-100.0,Max=-100.0),Y=(Min=-100.0,Max=100.0),Z=(Min=-100.0,Max=100.0))
	End Object
	Emitters(2)=SpriteEmitter'SpriteEmitter2'
	AutoDestroy=true
	bUseDynamicLights=false
	bNoDelete=false
	bAcceptsProjectors=false
	LifeSpan=100.0
}