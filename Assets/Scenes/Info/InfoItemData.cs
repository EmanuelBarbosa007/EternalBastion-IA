using UnityEngine;

// Define o tipo de categoria
public enum InfoCategory
{
    Torre,
    Tropa,
    Consumivel
}

[CreateAssetMenu(fileName = "NovoInfoItem", menuName = "TowerDefense/InfoItem")]
public class InfoItemData : ScriptableObject
{
    public string nomeItem;
    [TextArea] public string descricao;
    public Sprite imagem; // A imagem da torre/tropa
    public InfoCategory categoria; // A qual categoria pertence
}