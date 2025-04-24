[System.Serializable]
public struct PlayerData
{
    public int health;
    public float stamina;
    public int healing;

    public PlayerData(int health, float stamina, int healing)
    {
        this.health = health;
        this.stamina = stamina;
        this.healing = healing;
    }
}
