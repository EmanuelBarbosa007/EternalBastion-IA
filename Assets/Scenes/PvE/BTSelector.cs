using System.Collections.Generic;

public class BTSelector : BTNode
{
    protected List<BTNode> children;

    public BTSelector(AIController ai, List<BTNode> children) : base(ai)
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
                // Se um filho falhar, tentamos o próximo
                case NodeState.FAILURE:
                    continue;

                // Se um filho tiver sucesso, o Selector teve sucesso!
                case NodeState.SUCCESS:
                    return NodeState.SUCCESS;

                // Se um filho estiver "a correr", o Selector também está
                case NodeState.RUNNING:
                    return NodeState.RUNNING;
            }
        }

        // Se chegámos aqui, é porque todos os filhos falharam
        return NodeState.FAILURE;
    }
}