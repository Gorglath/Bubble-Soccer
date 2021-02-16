using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelGeneration/TileSet")]
public class TileSet : ScriptableObject
{
    public GameObject Block;

    public GameObject Bridge;

    public GameObject MiddleCenter;

    public GameObject StartingOrEnding;

    public GameObject TopCenter;

    public GameObject TopLeft;

    public GameObject SmallEnemy;

    public GameObject BigEnemy;

    public GameObject SpringTrap;

    public GameObject SpikedBallTrap;

    public GameObject TrapButton;

    public GameObject MovementBoost;

    public GameObject ShieldBoost;

    public GameObject PushBoost;

    public GameObject Cone;

    public GameObject Hole;

    public GameObject Player;

    public GameObject Goal;

    public GameObject Ball;

    public GameObject Shadow;

    public Color FenceColor;

    public Color WallColor;

    public Color GroundLightColor;

    public Color GroundDarkColor;
}
