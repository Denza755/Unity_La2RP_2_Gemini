class s_u013_c extends NCubics;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter2
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		Opacity=0.5
		FadeOutStartTime=10.0
		MaxParticles=1
		Name="SpriteEmitter8"
		SpinParticles=true
		StartSpinRange=(X=(Min=0.3,Max=0.3),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.46,RelativeSize=0.95)
		SizeScale(1)=(RelativeTime=1.0,RelativeSize=1.0)
		SizeScaleRepeats=99.0
		StartSizeRange=(X=(Min=4.0,Max=4.0),Y=(Min=4.0,Max=4.0),Z=(Min=4.0,Max=4.0))
		UniformSize=true
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		Texture=Texture'LineageEffectsTextures.Cubics.fx_m_t0091'
		TextureUSubdivisions=4
		TextureVSubdivisions=4
		SubdivisionEnd=1
		UseRandomSubdivision=true
		LifetimeRange=(Min=10.0,Max=10.0)
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter2'
	Begin Object Class=MeshEmitter Name=MeshEmitter1
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Cubics.cubic_heal00'
		UseMeshBlendMode=false
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.532143,Color=(B=255,G=255,R=255,A=200))
		ColorScale(2)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorScaleRepeats=99.0
		FadeOutStartTime=10.0
		MaxParticles=1
		Name="MeshEmitter9"
		ZWrite=true
		SpinParticles=true
		SpinCCWorCW=(X=0.0,Y=0.5,Z=0.5)
		SpinsPerSecondRange=(X=(Min=0.2,Max=0.2),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		StartSizeRange=(X=(Min=0.047,Max=0.047),Y=(Min=0.047,Max=0.047),Z=(Min=0.047,Max=0.047))
		UniformSize=true
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		LifetimeRange=(Min=10.0,Max=10.0)
	End Object
	Emitters(1)=MeshEmitter'MeshEmitter1'
	AutoReset=true
	Tag="Emitter"
	DrawScale=0.01
	bUnlit=false
}