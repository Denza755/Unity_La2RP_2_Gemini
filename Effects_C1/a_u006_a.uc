class a_u006_a extends Emitter;

defaultproperties
{
	Begin Object Class=MeshEmitter Name=MeshEmitter8
		StaticMesh=StaticMesh'FX_M_S.fx_hold.FX_head_plane20'
		UseMeshBlendMode=false
		RenderTwoSided=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=79,G=79,R=79,A=255))
		ColorMultiplierRange=(X=(Min=0.15,Max=0.15),Y=(Min=0.1,Max=0.1),Z=(Min=0.1,Max=0.1))
		Opacity=0.6
		FadeOutStartTime=0.98
		FadeOut=true
		FadeInEndTime=0.22
		FadeIn=true
		MaxParticles=3
		Name="MeshEmitter8"
		StartLocationOffset=(X=0.0,Y=0.0,Z=-2.0)
		SpinParticles=true
		SpinCCWorCW=(X=1.0,Y=1.0,Z=1.0)
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=1.0)
		SizeScale(1)=(RelativeTime=0.27,RelativeSize=0.8)
		SizeScale(2)=(RelativeTime=0.54,RelativeSize=0.7)
		SizeScale(3)=(RelativeTime=1.0,RelativeSize=0.5)
		StartSizeRange=(X=(Min=3.5,Max=3.5),Y=(Min=3.5,Max=3.5),Z=(Min=3.5,Max=3.5))
		InitialParticlesPerSecond=100.0
		DrawStyle=PTDS_AlphaBlend
		LifetimeRange=(Min=2.0,Max=2.0)
		StartVelocityRange=(X=(Min=0.0,Max=0.0),Y=(Min=0.0,Max=0.0),Z=(Min=1.2,Max=1.2))
	End Object
	Emitters(0)=MeshEmitter'MeshEmitter8'
	Begin Object Class=MeshEmitter Name=MeshEmitter13
		StaticMesh=StaticMesh'FX_M_S.fx_hold.FX_head_plane07'
		UseMeshBlendMode=false
		RenderTwoSided=true
		Acceleration=(X=0.0,Y=0.0,Z=1.0)
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		FadeOutStartTime=0.76
		FadeOut=true
		FadeInEndTime=0.64
		FadeIn=true
		MaxParticles=3
		Name="MeshEmitter13"
		StartLocationOffset=(X=0.0,Y=0.0,Z=4.0)
		SpinParticles=true
		StartSpinRange=(X=(Min=-1.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=1.0)
		SizeScale(1)=(RelativeTime=0.15,RelativeSize=0.8)
		SizeScale(2)=(RelativeTime=0.4,RelativeSize=0.5)
		SizeScale(3)=(RelativeTime=1.0,RelativeSize=0.3)
		StartSizeRange=(X=(Min=1.5,Max=1.5),Y=(Min=1.5,Max=1.5),Z=(Min=1.5,Max=1.5))
		LifetimeRange=(Min=2.0,Max=2.0)
	End Object
	Emitters(1)=MeshEmitter'MeshEmitter13'
	AutoDestroy=true
	Physics=10
	bNoDelete=false
	bTrailerSameRotation=true
	bTrailerPrePivot=true
	Tag="Emitter"
}