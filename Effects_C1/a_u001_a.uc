class a_u001_a extends Emitter;

defaultproperties
{
	Begin Object Class=MeshEmitter Name=MeshEmitter0
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Impact.bloodspray00'
		UseMeshBlendMode=false
		RenderTwoSided=true
		Acceleration=(X=-13.0,Y=0.0,Z=-50.0)
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorMultiplierRange=(X=(Min=0.4,Max=0.4),Y=(Min=0.4,Max=0.4),Z=(Min=0.4,Max=0.4))
		Opacity=0.6
		FadeOutStartTime=0.052
		FadeOut=true
		MaxParticles=4
		RespawnDeadParticles=false
		Name="MeshEmitter3"
		StartLocationOffset=(X=0.0,Y=0.0,Z=5.0)
		UseRotationFrom=1
		SpinParticles=true
		SpinCCWorCW=(X=0.5,Y=1.0,Z=0.5)
		SpinsPerSecondRange=(X=(Min=0.0,Max=0.0),Y=(Min=0.06,Max=0.06),Z=(Min=0.0,Max=0.0))
		StartSpinRange=(X=(Min=-0.15,Max=0.15),Y=(Min=-0.07,Max=0.02),Z=(Min=-0.05,Max=0.05))
		RotationNormal=(X=1.0,Y=0.0,Z=0.0)
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.11,RelativeSize=2.0)
		SizeScale(1)=(RelativeTime=0.28,RelativeSize=2.6)
		SizeScale(2)=(RelativeTime=0.58,RelativeSize=3.0)
		SizeScale(3)=(RelativeTime=1.0,RelativeSize=3.5)
		StartSizeRange=(X=(Min=0.015,Max=0.03),Y=(Min=0.028,Max=0.028),Z=(Min=0.028,Max=0.028))
		UniformSize=true
		InitialParticlesPerSecond=100.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_AlphaBlend
		LifetimeRange=(Min=0.2,Max=0.4)
		StartVelocityRange=(X=(Min=0.0,Max=0.0),Y=(Min=0.0,Max=0.0),Z=(Min=10.0,Max=10.0))
		GetVelocityDirectionFrom=PTVD_OwnerAndStartPosition
	End Object
	Emitters(0)=MeshEmitter'MeshEmitter0'
	AutoDestroy=true
	bRotEmitter=true
	RotPerSecond=(Pitch=0,Yaw=10000,Roll=0)
	bNoDelete=false
	Tag="Emitter"
}