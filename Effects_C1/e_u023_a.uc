class e_u023_a extends Emitter;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter16
		UseColorScale=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.625,Color=(B=70,G=70,R=70,A=255))
		ColorScale(2)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorScaleRepeats=20.0
		FadeOutStartTime=3.04
		FadeOut=true
		FadeInEndTime=0.72
		FadeIn=true
		CoordinateSystem=PTCS_Independent
		Name="SpriteEmitter16"
		StartLocationRange=(X=(Min=-50.0,Max=50.0),Y=(Min=-50.0,Max=50.0),Z=(Min=-20.0,Max=20.0))
		SphereRadiusRange=(Min=0.0,Max=100.0)
		StartLocationPolarRange=(X=(Min=0.0,Max=360.0),Y=(Min=5.0,Max=20.0),Z=(Min=0.0,Max=50.0))
		SpinParticles=true
		SpinCCWorCW=(X=1.0,Y=0.5,Z=0.5)
		SpinsPerSecondRange=(X=(Min=0.0,Max=0.3),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=0.1)
		SizeScale(1)=(RelativeTime=0.15,RelativeSize=1.0)
		SizeScale(2)=(RelativeTime=0.76,RelativeSize=1.0)
		SizeScale(3)=(RelativeTime=1.0,RelativeSize=0.1)
		StartSizeRange=(X=(Min=2.0,Max=4.0),Y=(Min=2.0,Max=4.0),Z=(Min=2.0,Max=4.0))
		UniformSize=true
		Texture=Texture'FX_E_T.particles_etc.elf_particleA_044'
		BlendBetweenSubdivisions=true
		StartVelocityRange=(X=(Min=-20.0,Max=20.0),Y=(Min=-20.0,Max=20.0),Z=(Min=-5.0,Max=5.0))
		WarmupTicksPerSecond=1.0
		RelativeWarmupTime=4.0
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter16'
	Physics=10
	bUseDynamicLights=false
	bLightChanged=true
	bNoDelete=false
	bTrailerPrePivot=true
	bAcceptsProjectors=false
	DrawScale=0.5
}