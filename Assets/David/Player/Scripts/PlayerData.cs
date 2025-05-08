[System.Serializable]
public class PlayerData
{
    public int health;
    public float stamina;
    public int healing;

    // Add level system data
    public LevelProgression levelProgression;

    public PlayerData(int health, float stamina, int healing)
    {
        this.health = health;
        this.stamina = stamina;
        this.healing = healing;
    }

    // New constructor with level data
    public PlayerData(int health, float stamina, int healing, LevelProgression progression)
    {
        this.health = health;
        this.stamina = stamina;
        this.healing = healing;
        this.levelProgression = progression;
    }
}