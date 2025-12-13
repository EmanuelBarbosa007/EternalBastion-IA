using UnityEngine;
using UnityEngine.UI;
using TMPro; // Se usares TextMeshPro (Recomendado)
using System.Collections.Generic;
using System.Linq; // Necessário para filtrar listas

public class InfoPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject infoPanelObject; // O Painel inteiro
    public Image displayImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;

    [Header("Data")]
    public List<InfoItemData> todosOsItens; // Arraste todos os seus ScriptableObjects para aqui

    // Variáveis internas para controlar o estado
    private List<InfoItemData> listaAtualFiltrada;
    private int indiceAtual = 0;

    void Start()
    {
        // Garante que o painel começa fechado
        infoPanelObject.SetActive(false);
        listaAtualFiltrada = new List<InfoItemData>();
    }

    // --- Funções de Abertura/Fecho ---

    public void TogglePanel()
    {
        bool estaAtivo = infoPanelObject.activeSelf;
        infoPanelObject.SetActive(!estaAtivo);

        if (!estaAtivo) // Se acabou de abrir
        {
            // Opcional: Abrir numa categoria padrão, ex: Torres
            MudarCategoria("Torre");
        }
    }

    // --- Lógica das Categorias ---

    // Associa esta função aos botões do topo (Torres, Tropas, etc.)
    // No botão Torres, escreve "Torre" no parâmetro do OnClick, etc.
    public void MudarCategoria(string categoriaNome)
    {
        // 1. Converter string para Enum
        InfoCategory catSelecionada;
        if (categoriaNome == "Torre") catSelecionada = InfoCategory.Torre;
        else if (categoriaNome == "Tropa") catSelecionada = InfoCategory.Tropa;
        else catSelecionada = InfoCategory.Consumivel;

        // 2. Filtrar a lista principal para criar uma sub-lista só com aquela categoria
        listaAtualFiltrada = todosOsItens.Where(x => x.categoria == catSelecionada).ToList();

        // 3. Resetar o índice e atualizar a tela
        indiceAtual = 0;
        AtualizarUI();
    }

    // --- Lógica de Navegação (Setas) ---

    public void ProximaPagina()
    {
        if (listaAtualFiltrada.Count == 0) return;

        indiceAtual++;
        // Lógica de Loop: Se passar do último, volta ao zero
        if (indiceAtual >= listaAtualFiltrada.Count)
        {
            indiceAtual = 0;
        }
        AtualizarUI();
    }

    public void PaginaAnterior()
    {
        if (listaAtualFiltrada.Count == 0) return;

        indiceAtual--;
        // Lógica de Loop: Se for menor que zero, vai para o último
        if (indiceAtual < 0)
        {
            indiceAtual = listaAtualFiltrada.Count - 1;
        }
        AtualizarUI();
    }

    // --- Atualização Visual ---

    private void AtualizarUI()
    {
        if (listaAtualFiltrada.Count > 0)
        {
            InfoItemData item = listaAtualFiltrada[indiceAtual];

            titleText.text = item.nomeItem;
            descriptionText.text = item.descricao;
            displayImage.sprite = item.imagem;

            // Garante que a imagem não fique distorcida
            displayImage.preserveAspect = true;
        }
        else
        {
            // Caso não haja itens nessa categoria
            titleText.text = "Vazio";
            descriptionText.text = "Nenhum item encontrado.";
            displayImage.sprite = null;
        }
    }
}