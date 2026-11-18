class RibbonSet extends Emitter;

defaultproperties
{
	Begin Object Class=RibbonEmitter Name=RibbonEmitter0
		SampleRate=0.002
		NumPoints=80
		AccDrop=3
		PointsDropRate=5
		MinPoints=20
		GetPointAxisFrom=3
		bUseInterpolation=true
		ScaleRatio=1.5
		bDecayPointsWhenStopped=true
		bSyncDecayWhenKilled=true
		bLengthBasedTextureU=true
		bUseBones=true
		Opacity=0.35
		FadeOutFactor=(W=2.0,X=2.0,Y=2.0,Z=2.0)
		FadeOut=true
		FadeInFactor=(W=0.5,X=1.2,Y=1.2,Z=1.2)
		FadeInEndTime=1.0
		FadeIn=true
		RespawnDeadParticles=false
		Name="RibbonEmitter0"
		ZWrite=true
		InitialParticlesPerSecond=100.0
		Texture=Texture'LineageEffectsTextures.Particles.fx_m_t0082'
	End Object
	Emitters(0)=RibbonEmitter'RibbonEmitter0'
	AutoDestroy=true
	bNoDelete=false
}