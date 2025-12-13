using UnityEngine;
using Unity.Netcode;

public class PvEAutoStart : MonoBehaviour
{
    [Header("Referências (Ligar no Inspector)")]

    [Tooltip("Arrasta para aqui o GameObject do GameManager da cena")]
    public GameManagerMP gameManager;

    [Tooltip("Arrasta para aqui o GameObject do PAINEL que tem os botões Host/Client")]
    public GameObject painelDeConexao;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManagerMP>();
        }

        // 2. Verifica se o GameManager existe e se está em modo PvE
        if (gameManager != null && gameManager.modoPvE)
        {
            // MODO PVE: Inicia como Host automaticamente
            Debug.Log("PvEAutoStart: Modo PvE detetado. A iniciar como Host...");

            // "Clica" no Host por ti
            NetworkManager.Singleton.StartHost();

            // Esconde o painel de conexão
            if (painelDeConexao != null)
            {
                painelDeConexao.SetActive(false);
            }
        }
        else
        {
            // MODO PVP: Mostra o painel normalmente
            if (painelDeConexao != null)
            {
                painelDeConexao.SetActive(true);
            }
        }
    }
}