using System.Collections;
using System.Collections.Generic;
using Managers;
using UnityEngine;
using Zenject;

public class Test : MonoBehaviour
{
    [Inject] public ManagerContext ManagerContext;
}
