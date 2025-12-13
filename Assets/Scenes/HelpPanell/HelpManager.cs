using UnityEngine;

public class HelpManager : MonoBehaviour
{
    [Header("Arrasta o Painel aqui no Inspector")]
    public GameObject painelAjuda;

    public void AlternarPainel()
    {

        bool estaAtivo = painelAjuda.activeSelf;

        // Define o estado oposto (se está ligado, desliga; se desligado, liga)
        painelAjuda.SetActive(!estaAtivo);
    }
}