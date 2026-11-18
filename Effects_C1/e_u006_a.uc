class e_u006_a extends Emitter;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter0
		Acceleration=(X=-50.0,Y=0.0,Z=120.0)
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.482143,Color=(B=210,G=210,R=210,A=255))
		ColorScale(2)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorScaleRepeats=5.0
		FadeOutStartTime=0.32
		FadeOut=true
		FadeInEndTime=0.032
		FadeIn=true
		MaxParticles=15
		RespawnDeadParticles=false
		Name="SpriteEmitter15"
		SpinParticles=true
		SpinsPerSecondRange=(X=(Min=-0.3,Max=0.3),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=1.0,RelativeSize=3.0)
		StartSizeRange=(X=(Min=4.0,Max=6.0),Y=(Min=4.0,Max=6.0),Z=(Min=4.0,Max=6.0))
		UniformSize=true
		InitialParticlesPerSecond=40.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0071'
		TextureUSubdivisions=4
		TextureVSubdivisions=4
		BlendBetweenSubdivisions=true
		SubdivisionStart=14
		SubdivisionEnd=16
		LifetimeRange=(Min=0.8,Max=0.8)
		StartVelocityRange=(X=(Min=15.0,Max=150.0),Y=(Min=-80.0,Max=80.0),Z=(Min=-20.0,Max=-10.0))
		VelocityLossRange=(X=(Min=4.0,Max=4.0),Y=(Min=4.0,Max=4.0),Z=(Min=0.0,Max=0.0))
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter0'
	AutoDestroy=true
	AutoReset=true
	AutoReplay=true
	bLightChanged=true
	bNoDelete=false
	Tag="Emitter"
	DrawScale=0.05
	bUnlit=false
}