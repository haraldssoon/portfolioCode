using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InfectaclesBaseState : State
{
    //Kan man skippa base-state? är det obsolete?
    //Hur får varje state tillgång till controllern utan att göra som nedan i varenda state?
    private InfectaclesController infectacles;
    public InfectaclesController Infectacles => infectacles = infectacles ?? (InfectaclesController)owner;
}
