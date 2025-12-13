using System;

public class BTAction : BTNode
{
    // Um "delegate" que aponta para a função de ação no AIController
    private Func<NodeState> action;

    public BTAction(AIController ai, Func<NodeState> action) : base(ai)
    {
        this.action = action;
    }

    public override NodeState Evaluate()
    {
        // Simplesmente chama a função que lhe foi passada
        return action();
    }
}