using System;
using System.Collections.Generic;

[System.Serializable]
public class SaveAllCharactersData
{
	
	public int score;
	public float health;
	public int money;
	public int missionID;
	public float gameTime;

    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;



    public List<EnemySaveData> enemies =
        new List<EnemySaveData>();
}