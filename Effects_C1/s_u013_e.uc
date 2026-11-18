class s_u013_e extends NCubics;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter16
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		Opacity=0.75
		FadeOutStartTime=10.0
		MaxParticles=1
		Name="SpriteEmitter4"
		SpinParticles=true
		SpinCCWorCW=(X=1.0,Y=0.5,Z=0.5)
		SpinsPerSecondRange=(X=(Min=0.1,Max=0.1),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.5,RelativeSize=0.85)
		SizeScale(1)=(RelativeTime=1.0,RelativeSize=1.0)
		SizeScaleRepeats=9.0
		StartSizeRange=(X=(Min=3.8,Max=3.8),Y=(Min=3.8,Max=3.8),Z=(Min=3.8,Max=3.8))
		UniformSize=true
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		Texture=Texture'LineageEffectsTextures.Cubics.fx_m_t0091'
		TextureUSubdivisions=4
		TextureVSubdivisions=4
		SubdivisionStart=4
		SubdivisionEnd=5
		UseRandomSubdivision=true
		LifetimeRange=(Min=10.0,Max=10.0)
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter16'
	Begin Object Class=MeshEmitter Name=MeshEmitter8
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Cubics.cubic_vipr00'
		UseMeshBlendMode=false
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.532143,Color=(B=255,G=255,R=255,A=200))
		ColorScale(2)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorScaleRepeats=99.0
		FadeOutStartTime=10.0
		MaxParticles=1
		Name="MeshEmitter2"
		ZWrite=true
		SpinParticles=true
		SpinCCWorCW=(X=1.0,Y=0.5,Z=0.5)
		SpinsPerSecondRange=(X=(Min=0.2,Max=0.2),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.1,RelativeSize=0.995)
		SizeScale(1)=(RelativeTime=0.5,RelativeSize=0.9)
		SizeScale(2)=(RelativeTime=0.6,RelativeSize=0.905)
		SizeScale(3)=(RelativeTime=1.0,RelativeSize=1.0)
		SizeScaleRepeats=9.0
		StartSizeRange=(X=(Min=0.042,Max=0.042),Y=(Min=0.042,Max=0.042),Z=(Min=0.042,Max=0.042))
		UniformSize=true
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		LifetimeRange=(Min=10.0,Max=10.0)
	End Object
	Emitters(1)=MeshEmitter'MeshEmitter8'
	AutoReset=true
	Tag="Emitter"
	DrawScale=0.01
	bUnlit=false
}