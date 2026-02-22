using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Se coloca directamente dentro de un bloque de animación en el Animator.
// Reproduce un sonido justo al TERMINAR (Salir) de una animación. Aún no implementado
public class PlaySoundExit : StateMachineBehaviour
{
    [SerializeField] private SoundType sound;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager.PlaySound(sound);
    }
}
