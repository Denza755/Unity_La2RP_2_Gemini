class s_u013_d extends NCubics;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter12
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		Opacity=0.6
		FadeOutStartTime=10.0
		MaxParticles=1
		Name="SpriteEmitter4"
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.12,RelativeSize=0.9)
		SizeScale(1)=(RelativeTime=0.22,RelativeSize=0.98)
		SizeScale(2)=(RelativeTime=0.32,RelativeSize=0.9)
		SizeScale(3)=(RelativeTime=1.0,RelativeSize=1.0)
		SizeScaleRepeats=9.0
		StartSizeRange=(X=(Min=3.8,Max=3.8),Y=(Min=3.8,Max=3.8),Z=(Min=3.8,Max=3.8))
		UniformSize=true
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		Texture=Texture'LineageEffectsTextures.Cubics.fx_m_t0091'
		TextureUSubdivisions=4
		TextureVSubdivisions=4
		SubdivisionStart=1
		SubdivisionEnd=2
		UseRandomSubdivision=true
		LifetimeRange=(Min=10.0,Max=10.0)
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter12'
	Begin Object Class=MeshEmitter Name=MeshEmitter6
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Cubics.cubic_vamp00'
		UseMeshBlendMode=false
		UseColorScale=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.532143,Color=(B=255,G=255,R=255,A=200))
		ColorScale(2)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorScaleRepeats=9.0
		FadeOutStartTime=10.0
		MaxParticles=1
		Name="MeshEmitter2"
		ZWrite=true
		SpinParticles=true
		SpinCCWorCW=(X=1.0,Y=0.5,Z=0.5)
		SpinsPerSecondRange=(X=(Min=0.1,Max=0.1),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.12,RelativeSize=0.93)
		SizeScale(1)=(RelativeTime=0.2,RelativeSize=1.0)
		SizeScale(2)=(RelativeTime=0.3,RelativeSize=0.9)
		SizeScale(3)=(RelativeTime=1.0,RelativeSize=1.0)
		SizeScaleRepeats=9.0
		StartSizeRange=(X=(Min=0.045,Max=0.045),Y=(Min=0.045,Max=0.045),Z=(Min=0.045,Max=0.045))
		UniformSize=true
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		LifetimeRange=(Min=10.0,Max=10.0)
	End Object
	Emitters(1)=MeshEmitter'MeshEmitter6'
	AutoReset=true
	Tag="Emitter"
	DrawScale=0.01
	bUnlit=false
}