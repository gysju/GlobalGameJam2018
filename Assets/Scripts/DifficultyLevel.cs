using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyLevel", menuName = "DifficultyLevel", order = 53)]
public class DifficultyLevel : ScriptableObject
{

    [Header("Interface")]
    public string Name;
    public string Description;
    public Sprite Image;

    [Header("Game Construction")]
    public int Lenght = 100;
    public int SegmentPointCount = 40;
    public int Segments = 5;
    public float Height = 50;
    public float MinLinkLenght = 4;
    public float MaxLinkLenght = 10;

    [Space(10), Header("Points type")]
    public float KillPoint = 0.1f;
    public float FriedPoint = 0.1f;
    public float BackPoint = 0.1f;

    [Space(10), Header("Other parameters")]
    public float PlayerSpeed = 10;
    public float DeathWaveSpeed = 4;
    public int EnemyCount = 0;

}
