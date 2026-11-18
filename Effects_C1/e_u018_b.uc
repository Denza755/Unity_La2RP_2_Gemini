class e_u018_b extends Emitter;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter5
		UseDirectionAs=PTDU_Normal
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		FadeOutStartTime=0.0106
		FadeOut=true
		CoordinateSystem=PTCS_Independent
		MaxParticles=1
		RespawnDeadParticles=false
		Name="SpriteEmitter5"
		SpinParticles=true
		SpinCCWorCW=(X=1.0,Y=0.5,Z=0.5)
		StartSpinRange=(X=(Min=0.0,Max=0.1),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=0.8)
		SizeScale(1)=(RelativeTime=0.14,RelativeSize=1.6)
		SizeScale(2)=(RelativeTime=0.25,RelativeSize=2.4)
		SizeScale(3)=(RelativeTime=0.41,RelativeSize=3.0)
		SizeScale(4)=(RelativeTime=0.67,RelativeSize=3.6)
		SizeScale(5)=(RelativeTime=1.0,RelativeSize=4.0)
		StartSizeRange=(X=(Min=12.0,Max=12.0),Y=(Min=12.0,Max=12.0),Z=(Min=12.0,Max=12.0))
		UniformSize=true
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		Texture=Texture'FX_E_T.Etc.water_wave00'
		LifetimeRange=(Min=0.65,Max=0.65)
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter5'
	AutoDestroy=true
	AutoReplay=true
	bNoDelete=false
	DrawScale=0.3
}