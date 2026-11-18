class s_u013_a extends NCubics;

defaultproperties
{
	Begin Object Class=SpriteEmitter Name=SpriteEmitter11
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		Opacity=0.45
		FadeOutStartTime=10.0
		MaxParticles=1
		Name="SpriteEmitter8"
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.46,RelativeSize=0.95)
		SizeScale(1)=(RelativeTime=1.0,RelativeSize=1.0)
		SizeScaleRepeats=79.0
		StartSizeRange=(X=(Min=4.0,Max=4.0),Y=(Min=4.0,Max=4.0),Z=(Min=4.0,Max=4.0))
		UniformSize=true
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		Texture=Texture'LineageEffectsTextures.Cubics.fx_m_t0091'
		TextureUSubdivisions=4
		TextureVSubdivisions=4
		SubdivisionStart=3
		SubdivisionEnd=4
		UseRandomSubdivision=true
		LifetimeRange=(Min=10.0,Max=10.0)
	End Object
	Emitters(0)=SpriteEmitter'SpriteEmitter11'
	Begin Object Class=MeshEmitter Name=MeshEmitter5
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Cubics.cubic_wind00'
		UseMeshBlendMode=false
		RenderTwoSided=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.314286,Color=(B=194,G=194,R=194,A=255))
		ColorScale(2)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorScaleRepeats=70.0
		FadeOutStartTime=10.0
		MaxParticles=1
		Name="MeshEmitter9"
		ZWrite=true
		SpinParticles=true
		SpinCCWorCW=(X=0.0,Y=0.5,Z=0.5)
		SpinsPerSecondRange=(X=(Min=3.0,Max=3.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		StartSizeRange=(X=(Min=0.018,Max=0.018),Y=(Min=0.018,Max=0.018),Z=(Min=0.018,Max=0.018))
		InitialParticlesPerSecond=100000.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		LifetimeRange=(Min=10.0,Max=10.0)
	End Object
	Emitters(1)=MeshEmitter'MeshEmitter5'
	AutoReset=true
	Tag="Emitter"
	DrawScale=0.01
	bUnlit=false
}