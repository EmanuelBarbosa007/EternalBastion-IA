using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using System.Net;
using System.Net.Sockets;
using TMPro;

public class NetworkConnectUI : MonoBehaviour
{
    [Header("Paineis")]
    public GameObject selectionPanel;
    public GameObject hostPanel;
    public GameObject clientPanel;

    [Header("Elementos do Painel de Seleção")]
    public Button selectHostButton;
    public Button selectClientButton;

    [Header("Elementos do Painel de Host")]
    public TextMeshProUGUI hostInfoText;

    [Header("Elementos do Painel de Client")]
    public TMP_InputField ipAddressField;
    public Button connectButton;

    void Start()
    {
        if (selectHostButton != null) selectHostButton.onClick.AddListener(OnSelectHost);
        if (selectClientButton != null) selectClientButton.onClick.AddListener(OnSelectClient);
        if (connectButton != null) connectButton.onClick.AddListener(OnConnect);

        selectionPanel.SetActive(true);
        hostPanel.SetActive(false);
        clientPanel.SetActive(false);

        if (ipAddressField != null) ipAddressField.text = "127.0.0.1";
    }

    public void OnSelectHost()
    {
        Debug.Log("Modo Host selecionado.");
        selectionPanel.SetActive(false);
        hostPanel.SetActive(true);

        string localIP = GetLocalIPv4();

        if (hostInfoText != null)
            hostInfoText.text = $"Host Ativo\nIP para conectar: {localIP}";

        // --- FIX: Configurar o Host para aceitar conexões externas ---
        var utpTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (utpTransport != null)
        {
            // Usar "0.0.0.0" permite escutar em todas as interfaces de rede do PC
            utpTransport.SetConnectionData("0.0.0.0", 7777);
        }

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartHost();
    }

    public void OnSelectClient()
    {
        selectionPanel.SetActive(false);
        clientPanel.SetActive(true);
    }

    public void OnConnect()
    {
        string ipAddress = "127.0.0.1";
        if (ipAddressField != null && !string.IsNullOrEmpty(ipAddressField.text))
        {
            ipAddress = ipAddressField.text;
        }

        Debug.Log($"Conectando a {ipAddress}...");

        var utpTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (utpTransport != null)
        {
            // O Cliente PRECISA do IP específico do Host
            utpTransport.SetConnectionData(ipAddress, 7777);
        }

        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.StartClient();

        clientPanel.SetActive(false);
    }

    private string GetLocalIPv4()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip))
                {
                    return ip.ToString();
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Erro IP: " + ex.Message);
        }
        return "Erro";
    }
}