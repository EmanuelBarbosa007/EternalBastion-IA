using System;

public class BTCheck : BTNode
{
    // Um "delegate" que aponta para a função de verificação no AIController
    private Func<bool> check;

    public BTCheck(AIController ai, Func<bool> check) : base(ai)
    {
        this.check = check;
    }

    public override NodeState Evaluate()
    {
        // Se a verificação for verdadeira, retorna Sucesso. Senão, Falha.
        return check() ? NodeState.SUCCESS : NodeState.FAILURE;
    }
}