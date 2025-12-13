using System.Collections.Generic;

// O estado de um nó após ser avaliado
public enum NodeState
{
    RUNNING, // O nó ainda está a trabalhar (ex: mover-se para um ponto)
    SUCCESS, // O nó completou a sua tarefa com sucesso
    FAILURE  // O nó falhou a sua tarefa
}

// Classe abstrata da qual todos os outros nós vão herdar
public abstract class BTNode
{
    // Todos os nós precisam de uma referência ao "cérebro" (o AIController)
    // para poderem aceder aos dados (dinheiro, spots, etc.)
    protected AIController ai;

    public BTNode(AIController ai)
    {
        this.ai = ai;
    }

    // O método principal que é chamado para "correr" o nó
    public abstract NodeState Evaluate();
}