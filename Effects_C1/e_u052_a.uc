class e_u052_a extends Emitter;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter6
		UseColorScale=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.503571,Color=(B=179,G=179,R=179,A=255))
		ColorScale(2)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorScaleRepeats=4.0
		ColorMultiplierRange=(X=(Min=0.65,Max=0.65),Y=(Min=0.594,Max=0.594),Z=(Min=0.429,Max=0.429))
		FadeOutStartTime=0.8
		FadeOut=true
		FadeInEndTime=0.4
		FadeIn=true
		MaxParticles=60
		ForcedMaxParticles=true
		RespawnDeadParticles=false
		Name="smoke"
		StartLocationOffset=(X=20.0,Y=0.0,Z=-10.0)
		StartLocationRange=(X=(Min=0.0,Max=0.0),Y=(Min=-15.0,Max=15.0),Z=(Min=0.0,Max=0.0))
		StartLocationPolarRange=(X=(Min=0.0,Max=360.0),Y=(Min=90.0,Max=90.0),Z=(Min=80.0,Max=250.0))
		SpinParticles=true
		SpinsPerSecondRange=(X=(Min=0.0,Max=0.1),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=1.0,RelativeSize=1.7)
		StartSizeRange=(X=(Min=25.0,Max=28.0),Y=(Min=25.0,Max=28.0),Z=(Min=25.0,Max=28.0))
		UniformSize=true
		InitialParticlesPerSecond=12.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0108'
		LifetimeRange=(Min=2.0,Max=2.0)
		StartVelocityRange=(X=(Min=0.0,Max=0.0),Y=(Min=0.0,Max=0.0),Z=(Min=15.0,Max=15.0))
		VelocityLossRange=(X=(Min=2.0,Max=3.0),Y=(Min=2.0,Max=3.0),Z=(Min=1.0,Max=1.0))
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter6'
	AutoDestroy=true
	Physics=10
	bLightChanged=true
	bNoDelete=false
	bTrailerSameRotation=true
	Tag="Emitter"
	DrawScale=2.0
	bUnlit=false
}