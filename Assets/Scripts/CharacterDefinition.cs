using FishNet.Object;
using UnityEngine;

[CreateAssetMenu(menuName = "MOBA/CharacterDefinition")]
public class CharacterDefinition : ScriptableObject
{
    public string characterName;
    public Sprite thumbnail;
    public NetworkObject playerPrefab;
}