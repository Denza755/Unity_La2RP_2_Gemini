class e_u034_b extends NSkillProjectile;

defaultproperties
{
	Speed=1000.0
	AccSpeed=3000.0
	Begin Object Class=SpriteEmitter Name=SpriteEmitter8
		ProjectionNormal=(X=1.0,Y=0.0,Z=0.0)
		UseColorScale=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.514286,Color=(B=128,G=128,R=128,A=255))
		ColorScale(2)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorScaleRepeats=20.0
		MaxParticles=20
		RespawnDeadParticles=false
		Name="Circle"
		StartLocationRange=(X=(Min=-38.0,Max=0.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.6,RelativeSize=1.0)
		SizeScale(1)=(RelativeTime=1.0,RelativeSize=0.5)
		StartSizeRange=(X=(Min=6.0,Max=6.0),Y=(Min=5.0,Max=5.0),Z=(Min=5.0,Max=5.0))
		UniformSize=true
		InitialParticlesPerSecond=10000.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_Brighten
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0000'
		TextureUSubdivisions=8
		TextureVSubdivisions=8
		SubdivisionStart=11
		SubdivisionEnd=12
		UseRandomSubdivision=true
		LifetimeRange=(Min=2.0,Max=2.0)
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter8'
	Begin Object Class=SpriteEmitter Name=SpriteEmitter9
		UseDirectionAs=PTDU_Up
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorMultiplierRange=(X=(Min=0.3,Max=0.3),Y=(Min=0.6,Max=0.6),Z=(Min=1.0,Max=1.0))
		MaxParticles=1
		RespawnDeadParticles=false
		Name="Stick"
		StartLocationOffset=(X=-20.0,Y=0.0,Z=0.0)
		StartSizeRange=(X=(Min=4.0,Max=4.0),Y=(Min=25.0,Max=25.0),Z=(Min=25.0,Max=25.0))
		InitialParticlesPerSecond=1000.0
		AutomaticInitialSpawning=false
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0000'
		TextureUSubdivisions=8
		TextureVSubdivisions=4
		SubdivisionStart=4
		SubdivisionEnd=5
		UseRandomSubdivision=true
		LifetimeRange=(Min=2.0,Max=2.0)
		StartVelocityRange=(X=(Min=0.1,Max=0.1),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
	End Object
	Emitters(1)=SpriteEmitter'SpriteEmitter9'
	AutoDestroy=true
	AutoReset=true
	Tag="Emitter"
	DrawScale=0.1
}