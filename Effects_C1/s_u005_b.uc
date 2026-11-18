class s_u005_b extends Emitter;

defaultproperties
{
	Begin Object Class=MeshEmitter Name=MeshEmitter1
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Etc.etcpotion01'
		UseMeshBlendMode=false
		RenderTwoSided=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorMultiplierRange=(X=(Min=1.0,Max=1.0),Y=(Min=0.9,Max=0.9),Z=(Min=0.6,Max=0.6))
		Opacity=0.77
		FadeOutStartTime=0.087
		FadeOut=true
		MaxParticles=1
		RespawnDeadParticles=false
		Name="MeshEmitter1"
		StartLocationOffset=(X=0.0,Y=0.0,Z=10.0)
		SpinParticles=true
		SpinCCWorCW=(X=0.0,Y=0.5,Z=0.5)
		SpinsPerSecondRange=(X=(Min=-3.0,Max=-3.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=1.0,RelativeSize=2.0)
		StartSizeRange=(X=(Min=0.3,Max=0.3),Y=(Min=0.3,Max=0.3),Z=(Min=0.05,Max=0.05))
		InitialParticlesPerSecond=20.0
		AutomaticInitialSpawning=false
		LifetimeRange=(Min=0.3,Max=0.3)
		StartVelocityRange=(X=(Min=0.0,Max=0.0),Y=(Min=0.0,Max=0.0),Z=(Min=60.0,Max=60.0))
	End Object
	Emitters(0)=MeshEmitter'MeshEmitter1'
	Begin Object Class=MeshEmitter Name=MeshEmitter4
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Skill_Power.skill_charge00'
		UseMeshBlendMode=false
		RenderTwoSided=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=1.0,Color=(B=255,G=255,R=255,A=255))
		ColorMultiplierRange=(X=(Min=1.0,Max=1.0),Y=(Min=0.713,Max=0.713),Z=(Min=0.645,Max=0.645))
		FadeOut=true
		MaxParticles=1
		RespawnDeadParticles=false
		Name="MeshEmitter4"
		StartLocationOffset=(X=0.0,Y=0.0,Z=8.0)
		SpinParticles=true
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.07,RelativeSize=1.6)
		SizeScale(1)=(RelativeTime=0.2,RelativeSize=2.2)
		SizeScale(2)=(RelativeTime=0.43,RelativeSize=2.8)
		SizeScale(3)=(RelativeTime=1.0,RelativeSize=3.4)
		StartSizeRange=(X=(Min=0.17,Max=0.17),Y=(Min=0.17,Max=0.17),Z=(Min=0.02,Max=0.02))
		InitialParticlesPerSecond=15.0
		AutomaticInitialSpawning=false
		LifetimeRange=(Min=0.2,Max=0.2)
	End Object
	Emitters(1)=MeshEmitter'MeshEmitter4'
	AutoDestroy=true
	AutoReset=true
	Physics=10
	bUseDynamicLights=false
	bLightChanged=true
	bNoDelete=false
	bTrailerPrePivot=true
	bAcceptsProjectors=false
	DrawScale=0.05
}