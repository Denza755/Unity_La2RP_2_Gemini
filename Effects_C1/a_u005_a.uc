class a_u005_a extends Emitter;

defaultproperties
{
	Begin Object Class=MeshEmitter Name=MeshEmitter4
		StaticMesh=StaticMesh'FX_M_S.fx_hold.FX_head_plane1222'
		UseMeshBlendMode=false
		RenderTwoSided=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		FadeOutStartTime=4.0
		MaxParticles=1
		Name="MeshEmitter0"
		StartLocationOffset=(X=0.0,Y=0.0,Z=4.5)
		SpinParticles=true
		SpinCCWorCW=(X=1.0,Y=1.0,Z=1.0)
		SpinsPerSecondRange=(X=(Min=0.05,Max=0.05),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		StartSizeRange=(X=(Min=0.85,Max=0.85),Y=(Min=0.85,Max=0.85),Z=(Min=0.6,Max=0.6))
		InitialParticlesPerSecond=10000.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_Darken
		LifetimeRange=(Min=100000.0,Max=100000.0)
	End Object
	Emitters(0)=MeshEmitter'MeshEmitter4'
	Begin Object Class=SpriteEmitter Name=SpriteEmitter2
		UseColorScale=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.117857,Color=(B=255,G=255,R=255,A=255))
		ColorScale(2)=(RelativeTime=0.696429,Color=(B=128,G=128,R=128,A=255))
		ColorScale(3)=(RelativeTime=1.0,Color=(B=138,G=138,R=138,A=255))
		ColorScaleRepeats=5.0
		ColorMultiplierRange=(X=(Min=0.6,Max=1.0),Y=(Min=0.6,Max=1.0),Z=(Min=0.6,Max=1.0))
		FadeOutStartTime=0.8142
		FadeOut=true
		MaxParticles=6
		Name="SpriteEmitter5"
		StartLocationOffset=(X=0.0,Y=0.0,Z=3.0)
		StartLocationShape=PTLS_Polar
		StartLocationPolarRange=(X=(Min=0.0,Max=360.0),Y=(Min=90.0,Max=90.0),Z=(Min=6.3,Max=6.3))
		SpinParticles=true
		StartSpinRange=(X=(Min=0.1,Max=0.1),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		StartSizeRange=(X=(Min=5.0,Max=5.0),Y=(Min=5.0,Max=5.0),Z=(Min=5.0,Max=5.0))
		UniformSize=true
		InitialParticlesPerSecond=10000.0
		Texture=Texture'FX_M_T.M_FX_particle.fx_head_texture011'
		TextureUSubdivisions=2
		TextureVSubdivisions=2
		SubdivisionStart=2
		SubdivisionEnd=2
		LifetimeRange=(Min=1.0,Max=1.4)
	End Object
	Emitters(1)=SpriteEmitter'SpriteEmitter2'
	AutoDestroy=true
	bRotEmitter=true
	RotPerSecond=(Pitch=0,Yaw=5000,Roll=0)
	bNoDelete=false
	Tag="Emitter"
}