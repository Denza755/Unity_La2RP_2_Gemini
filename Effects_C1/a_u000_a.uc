class a_u000_a extends Emitter;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter1
		Acceleration=(X=0.0,Y=0.0,Z=-100.0)
		UseColorScale=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=140,G=64,R=135,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=120,G=63,R=106,A=255))
		ColorMultiplierRange=(X=(Min=0.8,Max=1.0),Y=(Min=0.8,Max=1.0),Z=(Min=0.8,Max=1.0))
		FadeOutStartTime=1.08
		FadeOut=true
		FadeInEndTime=0.12
		FadeIn=true
		CoordinateSystem=PTCS_Independent
		MaxParticles=20
		RespawnDeadParticles=false
		Name="Poison"
		StartLocationOffset=(X=0.0,Y=0.0,Z=-3.0)
		StartLocationRange=(X=(Min=-8.0,Max=8.0),Y=(Min=-8.0,Max=8.0),Z=(Min=0.0,Max=0.0))
		SpinParticles=true
		SpinsPerSecondRange=(X=(Min=0.05,Max=0.2),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.1,RelativeSize=3.0)
		SizeScale(1)=(RelativeTime=0.36,RelativeSize=4.6)
		SizeScale(2)=(RelativeTime=1.0,RelativeSize=6.0)
		StartSizeRange=(X=(Min=2.0,Max=4.5),Y=(Min=2.0,Max=4.5),Z=(Min=2.0,Max=4.5))
		UniformSize=true
		InitialParticlesPerSecond=30.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0089'
		TextureUSubdivisions=2
		TextureVSubdivisions=2
		BlendBetweenSubdivisions=true
		SubdivisionEnd=4
		LifetimeRange=(Min=1.0,Max=2.0)
		StartVelocityRange=(X=(Min=-80.0,Max=80.0),Y=(Min=-80.0,Max=80.0),Z=(Min=30.0,Max=30.0))
		VelocityLossRange=(X=(Min=4.0,Max=4.0),Y=(Min=4.0,Max=4.0),Z=(Min=4.0,Max=4.0))
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter1'
	AutoDestroy=true
	bNoDelete=false
	DrawScale=0.2
}