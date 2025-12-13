using System.Collections.Generic;

public class BTSequence : BTNode
{
    protected List<BTNode> children;

    public BTSequence(AIController ai, List<BTNode> children) : base(ai)
    {
        this.children = children;
    }

    public override NodeState Evaluate()
    {
        // Itera por todos os filhos
        foreach (BTNode node in children)
        {
            switch (node.Evaluate())
            {
                // Se um filho falhar, a Sequência inteira falha
                case NodeState.FAILURE:
                    return NodeState.FAILURE;

                // Se um filho tiver sucesso, passamos ao próximo
                case NodeState.SUCCESS:
                    continue;

                // Se um filho estiver "a correr", a Sequência também está
                case NodeState.RUNNING:
                    return NodeState.RUNNING;
            }
        }

        // Se chegámos aqui, é porque todos os filhos tiveram sucesso
        return NodeState.SUCCESS;
    }
}