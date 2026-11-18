class s_u013_x extends Emitter;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter1
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		Opacity=0.4
		FadeOutStartTime=0.5
		FadeInEndTime=0.075
		FadeIn=true
		MaxParticles=30
		ForcedMaxParticles=true
		RespawnDeadParticles=false
		Name="SpriteEmitter1"
		StartLocationShape=PTLS_Sphere
		SpinParticles=true
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=1.0,RelativeSize=0.3)
		StartSizeRange=(X=(Min=7.0,Max=7.0),Y=(Min=7.0,Max=7.0),Z=(Min=7.0,Max=7.0))
		UniformSize=true
		InitialParticlesPerSecond=10.0
		AutomaticInitialSpawning=false
		Texture=Texture'LineageEffectsTextures.Cubics.fx_m_t0091'
		TextureUSubdivisions=4
		TextureVSubdivisions=4
		SubdivisionStart=15
		SubdivisionEnd=16
		UseRandomSubdivision=true
		LifetimeRange=(Min=0.5,Max=0.5)
		StartVelocityRange=(X=(Min=20.0,Max=20.0),Y=(Min=20.0,Max=20.0),Z=(Min=20.0,Max=20.0))
		GetVelocityDirectionFrom=PTVD_StartPositionAndOwner
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter1'
	AutoDestroy=true
	AutoReset=true
	Physics=10
	bNoDelete=false
	bTrailerPrePivot=true
	Tag="Emitter"
	DrawScale=0.01
	bUnlit=false
}