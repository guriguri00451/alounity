using UnityEngine;
using FishRumble;
/// <summary>
/// Player(ボート一席分)のデータ、スポーン時に持たせる
/// </summary>
public class PlayerData: ScriptableObject
{
    [SerializeField] int playerID;
    // [SerializeField] 個別のInput 
    [SerializeField] int boatMaterialID;
    [SerializeField] int[] manMaterialID;

}