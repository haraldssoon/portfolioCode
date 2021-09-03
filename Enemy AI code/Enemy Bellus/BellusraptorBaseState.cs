using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BellusraptorBaseState : State
{
    //Kan man skippa base-state? är det obsolete?
    //Hur får varje state tillgång till controllern utan att göra som nedan i varenda state?
    private BellusraptorController bellusraptor;
    public BellusraptorController Bellusraptor => bellusraptor = bellusraptor ?? (BellusraptorController)owner;

}
