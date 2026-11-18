class m_u006_d extends Emitter;

defaultproperties
{
	Begin Object Class=MeshEmitter Name=MeshEmitter4
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Fire.firebomb01'
		UseMeshBlendMode=false
		RenderTwoSided=true
		UseColorScale=true
		ColorScale(0)=(RelativeTime=0.0,Color=(B=255,G=255,R=255,A=255))
		ColorScale(1)=(RelativeTime=0.275,Color=(B=255,G=255,R=255,A=255))
		ColorScale(2)=(RelativeTime=0.703571,Color=(B=41,G=40,R=96,A=255))
		ColorScale(3)=(RelativeTime=1.0,Color=(B=1,G=0,R=57,A=255))
		Opacity=0.6
		FadeOutStartTime=0.32
		FadeOut=true
		MaxParticles=1
		RespawnDeadParticles=false
		Name="MeshEmitter4"
		StartLocationOffset=(X=0.0,Y=0.0,Z=20.0)
		SpinParticles=true
		StartSpinRange=(X=(Min=0.0,Max=1.0),Y=(Min=0.0,Max=0.0),Z=(Min=0.0,Max=0.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		SizeScale(0)=(RelativeTime=0.0,RelativeSize=1.0)
		SizeScale(1)=(RelativeTime=0.1,RelativeSize=10.0)
		SizeScale(2)=(RelativeTime=0.33,RelativeSize=18.0)
		SizeScale(3)=(RelativeTime=0.6,RelativeSize=22.0)
		SizeScale(4)=(RelativeTime=1.0,RelativeSize=25.0)
		StartSizeRange=(X=(Min=0.1,Max=0.1),Y=(Min=0.1,Max=0.1),Z=(Min=0.05,Max=0.05))
		InitialParticlesPerSecond=60.0
		AutomaticInitialSpawning=false
		DrawStyle=PTDS_Brighten
		LifetimeRange=(Min=0.4,Max=0.4)
	End Object
	Emitters(0)=MeshEmitter'MeshEmitter4'
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