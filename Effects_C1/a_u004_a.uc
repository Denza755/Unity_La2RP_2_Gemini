class a_u004_a extends Emitter;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter5
		ColorScale(0)=(RelativeTime=0.0,Color=(B=185,G=185,R=185,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		Opacity=0.6
		FadeOutStartTime=1.14
		FadeOut=true
		FadeInEndTime=0.4
		FadeIn=true
		MaxParticles=3
		Name="SpriteEmitter5"
		StartLocationOffset=(X=0.0,Y=0.0,Z=8.0)
		SpinParticles=true
		SpinCCWorCW=(X=1.0,Y=0.5,Z=0.5)
		SpinsPerSecondRange=(X=(Min=0.0,Max=0.2),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=0.4)
		SizeScale(1)=(RelativeTime=1.0,RelativeSize=1.0)
		StartSizeRange=(X=(Min=9.0,Max=9.0),Y=(Min=9.0,Max=9.0),Z=(Min=9.0,Max=9.0))
		UniformSize=true
		InitialParticlesPerSecond=100.0
		DrawStyle=PTDS_AlphaBlend
		Texture=Texture'FX_M_T.M_FX_particle.fx_head_texture22'
		TextureUSubdivisions=1
		TextureVSubdivisions=1
		SubdivisionStart=1
		SubdivisionEnd=1
		LifetimeRange=(Min=2.0,Max=2.0)
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter5'
	AutoDestroy=true
	bNoDelete=false
	Tag="Emitter"
}