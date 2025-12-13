using UnityEngine;
using Unity.Netcode;

public class CameraControllerMP : NetworkBehaviour
{
    // --- Variáveis de Movimento e Zoom ---
    [Header("Movement Settings")]
    public float panSpeed = 20f;
    public float scrollSpeed = 20f;
    public float minY = 15f;
    public float maxY = 80f;

    [Header("Map Limits")]
    public Vector2 panLimitMin;
    public Vector2 panLimitMax;

    public override void OnNetworkSpawn()
    {

        if (PlayerPrefs.HasKey("Sensitivity"))
        {
            SettingsMenu.mouseSensitivity = PlayerPrefs.GetFloat("Sensitivity");
        }


        // Ativa a câmara e o audio listener APENAS para o jogador local
        if (IsOwner)
        {
            gameObject.name = $"CameraController - Local - ID {OwnerClientId}";
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = true;
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = true;

            // Define a posição inicial com uma altura média
            float startY = (minY + maxY) / 2.0f;

            // Define posições de spawn diferentes para Host e Cliente
            if (OwnerClientId == NetworkManager.ServerClientId) // Host (Jogador A)
            {
                transform.position = new Vector3(panLimitMin.x + 10, startY, panLimitMin.y + 10);
            }
            else // Cliente (Jogador B)
            {
                transform.position = new Vector3(panLimitMax.x - 10, startY, panLimitMax.y - 10);
            }
        }
        // Desativa a câmara e o audio listener para jogadores remotos
        else
        {
            gameObject.name = $"CameraController - Remote - ID {OwnerClientId}";
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) cam.enabled = false;
            AudioListener listener = GetComponentInChildren<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }

    void Update()
    {
        // Só executa o movimento se esta câmara pertencer ao jogador local
        if (!IsOwner)
        {
            return;
        }

        // Lógica de Movimento
        float xInput = 0f;
        float zInput = 0f;

        // 1. Obter Input
        if (Input.GetKey(KeyCode.W)) zInput -= 1f;
        if (Input.GetKey(KeyCode.S)) zInput += 1f;
        if (Input.GetKey(KeyCode.A)) xInput += 1f;
        if (Input.GetKey(KeyCode.D)) xInput -= 1f;

        // 2. Normalizar o vetor de direção 
        Vector3 dir = new Vector3(xInput, 0, zInput).normalized;


        // Aplicamos o multiplicador de sensibilidade vindo do menu de definições
        float currentPanSpeed = panSpeed * SettingsMenu.mouseSensitivity;

        Vector3 move = dir * currentPanSpeed * Time.deltaTime;

        // 3. Obter Posição Atual e Aplicar Movimento
        Vector3 newPos = transform.position;
        newPos += move;

        // 4. Zoom (Scroll)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        newPos.y -= scroll * scrollSpeed * 100f * Time.deltaTime;

        // 5. Aplicar Limites
        newPos.x = Mathf.Clamp(newPos.x, panLimitMin.x, panLimitMax.x);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY); // Limites de zoom (altura)
        newPos.z = Mathf.Clamp(newPos.z, panLimitMin.y, panLimitMax.y); // Limites de movimento Z

        transform.position = newPos;
    }
}