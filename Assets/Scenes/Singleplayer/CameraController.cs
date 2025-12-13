using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;        // Velocidade base de movimento
    public float zoomSpeed = 200f;       // Velocidade do scroll para zoom
    public float minY = 10f;             // Altura mínima (zoom in)
    public float maxY = 25f;             // Altura máxima (zoom out)

    [Header("Map Limits")]
    public float minX = -30f;
    public float maxX = 30f;
    public float minZ = -15f;
    public float maxZ = 30f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;

        // Ajusta posição inicial 
        transform.position = new Vector3(-27.5f, 20f, 20f);
        transform.rotation = Quaternion.Euler(60f, 180f, 0f);

        if (PlayerPrefs.HasKey("Sensitivity"))
        {
            SettingsMenu.mouseSensitivity = PlayerPrefs.GetFloat("Sensitivity");
        }
    }

    void Update()
    {
        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        float x = 0f;
        float z = 0f;

        // Movimento WASD
        if (Input.GetKey(KeyCode.W)) z -= 1f;
        if (Input.GetKey(KeyCode.S)) z += 1f;
        if (Input.GetKey(KeyCode.A)) x += 1f;
        if (Input.GetKey(KeyCode.D)) x -= 1f;

        Vector3 dir = new Vector3(x, 0, z).normalized;


        // Multiplicamos a velocidade base pela sensibilidade que vem do Menu de Definições
        // Se a classe SettingsMenu ainda não tiver sido corrida, o valor padrão lá é 1.0f
        float currentSpeed = moveSpeed * SettingsMenu.mouseSensitivity;

        Vector3 move = dir * currentSpeed * Time.deltaTime;

        // Move e limita dentro do mapa
        Vector3 newPos = transform.position + move;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.z = Mathf.Clamp(newPos.z, minZ, maxZ);

        transform.position = newPos;
    }

    void ZoomCamera()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * zoomSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }
    }
}