class e_u046_1l extends Emitter;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter20
		UseColorScale=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.457143,Color=(B=128,G=128,R=128,A=255))
		ColorScale(2)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorScaleRepeats=12.0
		Opacity=0.1
		FadeOutStartTime=0.165
		FadeOut=true
		FadeInEndTime=0.015
		FadeIn=true
		RespawnDeadParticles=false
		Name="EyeSpriteL2"
		StartLocationRange=(X=(Min=0.0,Max=0.0),Y=(Min=-5.0,Max=5.0),Z=(Min=-5.0,Max=5.0))
		SpinParticles=true
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.2,RelativeSize=0.5)
		SizeScale(1)=(RelativeTime=1.0,RelativeSize=0.3)
		StartSizeRange=(X=(Min=40.0,Max=40.0),Y=(Min=40.0,Max=40.0),Z=(Min=40.0,Max=40.0))
		UniformSize=true
		InitialParticlesPerSecond=1000.0
		AutomaticInitialSpawning=false
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0105'
		LifetimeRange=(Min=1.2,Max=1.2)
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter20'
	AutoDestroy=true
	AutoReset=true
	bLightChanged=true
	bNoDelete=false
	Tag="Emitter"
	DrawScale=0.1
	bUnlit=false
}