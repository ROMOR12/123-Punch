using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Se coloca directamente dentro de un bloque de animación en el Animator.
// Reproduce un sonido justo al EMPEZAR una animación. Aún no implementado
public class PlaySoundEnter : StateMachineBehaviour
{
    [SerializeField] private SoundType sound;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager.PlaySound(sound);
    }
}
