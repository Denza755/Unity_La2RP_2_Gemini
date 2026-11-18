class e_u018_a extends Emitter;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter2
		UseDirectionAs=PTDU_Normal
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		Opacity=0.65
		FadeOutStartTime=0.588
		FadeOut=true
		FadeInEndTime=0.204
		FadeIn=true
		CoordinateSystem=PTCS_Independent
		MaxParticles=1
		RespawnDeadParticles=false
		Name="SpriteEmitter2"
		SpinParticles=true
		SpinCCWorCW=(X=1.0,Y=0.5,Z=0.5)
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=0.6)
		SizeScale(1)=(RelativeTime=1.0,RelativeSize=2.0)
		StartSizeRange=(X=(Min=13.0,Max=13.0),Y=(Min=13.0,Max=13.0),Z=(Min=13.0,Max=13.0))
		UniformSize=true
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		Texture=Texture'FX_E_T.Etc.water_wave00'
		LifetimeRange=(Min=1.2,Max=1.2)
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter2'
	AutoDestroy=true
	AutoReplay=true
	bNoDelete=false
	DrawScale=0.35
}