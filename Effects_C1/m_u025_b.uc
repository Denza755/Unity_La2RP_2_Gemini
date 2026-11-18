class m_u025_b extends Emitter;

defaultproperties
{
	Begin Object Class=MeshEmitter Name=MeshEmitter10
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Skill_Power.skill_power01'
		UseMeshBlendMode=false
		RenderTwoSided=true
		UseColorScale=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.125,Color=(B=255,G=255,R=255,A=255))
		ColorScale(2)=(RelativeTime=0.425,Color=(B=128,G=128,R=128,A=255))
		ColorScale(3)=(RelativeTime=1.0,Color=(B=0,G=0,R=0,A=255))
		ColorMultiplierRange=(X=(Min=1.0,Max=1.0),Y=(Min=0.714,Max=0.714),Z=(Min=1.0,Max=1.0))
		Opacity=0.6
		FadeOutStartTime=0.3
		MaxParticles=5
		RespawnDeadParticles=false
		Name="Needle"
		SpinParticles=true
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=1.0)
		SizeScale(1)=(RelativeTime=1.0,RelativeSize=6.0)
		StartSizeRange=(X=(Min=0.15,Max=0.15),Y=(Min=0.15,Max=0.15),Z=(Min=0.06,Max=0.06))
		InitialParticlesPerSecond=30.0
		AutomaticInitialSpawning=false
		LifetimeRange=(Min=0.5,Max=0.5)
		VelocityLossRange=(X=(Min=0.0,Max=0.0),Y=(Min=0.0,Max=0.0),Z=(Min=4.0,Max=4.0))
	End Object
	Emitters(0)=MeshEmitter'MeshEmitter10'
	Begin Object Class=SpriteEmitter Name=SpriteEmitter9
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorMultiplierRange=(X=(Min=0.767,Max=1.0),Y=(Min=0.767,Max=1.0),Z=(Min=0.767,Max=1.0))
		Opacity=0.8
		FadeOutStartTime=0.04
		FadeOut=true
		MaxParticles=5
		RespawnDeadParticles=false
		Name="Cloud"
		SpinParticles=true
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.08,RelativeSize=2.0)
		SizeScale(1)=(RelativeTime=0.27,RelativeSize=4.0)
		SizeScale(2)=(RelativeTime=0.66,RelativeSize=6.0)
		SizeScale(3)=(RelativeTime=1.0,RelativeSize=7.0)
		StartSizeRange=(X=(Min=6.0,Max=6.0),Y=(Min=6.0,Max=6.0),Z=(Min=6.0,Max=6.0))
		UniformSize=true
		InitialParticlesPerSecond=30.0
		AutomaticInitialSpawning=false
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0067'
		TextureUSubdivisions=2
		TextureVSubdivisions=4
		SubdivisionStart=2
		SubdivisionEnd=4
		UseRandomSubdivision=true
		LifetimeRange=(Min=0.5,Max=0.5)
	End Object
	Emitters(1)=SpriteEmitter'SpriteEmitter9'
	AutoDestroy=true
	AutoReplay=true
	bNoDelete=false
	DrawScale=0.1
}