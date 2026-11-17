package com.shnok.javaserver.gameserver.model.ruler;

public class RulerBuildingData
{
	private int _id;
	private String _prefabName;
	private double _posX;
	private double _posY;
	private double _posZ;
	private float _rotY;
	private float _scale;
	private String _ownerName;
	private long _createdAt;
	
	public RulerBuildingData(int id, String prefabName, double posX, double posY, double posZ, float rotY, float scale, String ownerName, long createdAt)
	{
		_id = id;
		_prefabName = prefabName;
		_posX = posX;
		_posY = posY;
		_posZ = posZ;
		_rotY = rotY;
		_scale = scale;
		_ownerName = ownerName;
		_createdAt = createdAt;
	}
	
	public int getId()
	{
		return _id;
	}
	
	public void setId(int id)
	{
		_id = id;
	}
	
	public String getPrefabName()
	{
		return _prefabName;
	}
	
	public double getPosX()
	{
		return _posX;
	}
	
	public double getPosY()
	{
		return _posY;
	}
	
	public double getPosZ()
	{
		return _posZ;
	}
	
	public float getRotY()
	{
		return _rotY;
	}
	
	public float getScale()
	{
		return _scale;
	}
	
	public String getOwnerName()
	{
		return _ownerName;
	}
	
	public long getCreatedAt()
	{
		return _createdAt;
	}
}
