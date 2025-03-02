using UnityEngine;

[System.Serializable]
public class User
{
    public string id;
    public string name;
    public int coin;
    public int gem;

    public User(string id, string name,int coin,int gem)
    {
        this.id = id;
        this.name = name;
        this.coin = coin; // Default coins
        this.gem = gem; // Default gems
    }
}
