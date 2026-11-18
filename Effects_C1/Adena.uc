class Adena extends Emitter;

defaultproperties
{
	Begin Object Class=MeshEmitter Name=MeshEmitter1
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Etc.coin01'
		RenderTwoSided=true
		Acceleration=(X=0.0,Y=0.0,Z=-200.0)
		ColorMultiplierRange=(X=(Min=0.9,Max=0.9),Y=(Min=0.95,Max=0.95),Z=(Min=1.0,Max=1.0))
		CoordinateSystem=PTCS_Independent
		MaxParticles=20
		RespawnDeadParticles=false
		AutoDestroy=true
		Name="MeshEmitter1"
		StartLocationShape=PTLS_Sphere
		SphereRadiusRange=(Min=2.0,Max=2.0)
		SpinParticles=true
		SpinCCWorCW=(X=0.0,Y=0.5,Z=0.5)
		SpinsPerSecondRange=(X=(Min=0.0,Max=0.0),Y=(Min=3.0,Max=6.0),Z=(Min=0.0,Max=0.0))
		StartSpinRange=(X=(Min=0.0,Max=360.0),Y=(Min=360.0,Max=0.0),Z=(Min=0.0,Max=360.0))
		UseSizeScale=true
		UseRegularSizeScale=false
		StartSizeRange=(X=(Min=0.6,Max=0.6),Y=(Min=1.0,Max=1.0),Z=(Min=1.0,Max=1.0))
		UniformSize=true
		InitialParticlesPerSecond=80.0
		SecondsBeforeInactive=1.0
		LifetimeRange=(Min=1.0,Max=1.0)
		StartVelocityRange=(X=(Min=0.0,Max=0.0),Y=(Min=-40.0,Max=-20.0),Z=(Min=0.0,Max=0.0))
	End Object
	Emitters(0)=MeshEmitter'MeshEmitter1'
	Begin Object Class=MeshEmitter Name=MeshEmitter2
		StaticMesh=StaticMesh'LineageEffectsStaticmeshes.Etc.coin01'
		RenderTwoSided=true
		Acceleration=(X=0.0,Y=0.0,Z=-200.0)
		ColorMultiplierRange=(X=(Min=0.9,Max=0.9),Y=(Min=0.95,Max=0.95),Z=(Min=1.0,Max=1.0))
		CoordinateSystem=PTCS_Independent
		AutoDestroy=true
		Name="MeshEmitter2"
		StartLocationOffset=(X=0.0,Y=0.0,Z=3.0)
		StartLocationShape=PTLS_Sphere
		SphereRadiusRange=(Min=2.0,Max=2.0)
		SpinCCWorCW=(X=0.0,Y=0.5,Z=0.5)
		UseRegularSizeScale=false
		StartSizeRange=(X=(Min=3.0,Max=3.0),Y=(Min=1.0,Max=1.0),Z=(Min=1.0,Max=1.0))
		UniformSize=true
		SecondsBeforeInactive=1.0
		StartVelocityRange=(X=(Min=0.0,Max=0.0),Y=(Min=-40.0,Max=-20.0),Z=(Min=0.0,Max=0.0))
	End Object
	Emitters(1)=MeshEmitter'MeshEmitter2'
	AutoDestroy=true
	bNoDelete=false
	bTrailerSameRotation=true
	LifeSpan=40.0
	Mass=4.0
}