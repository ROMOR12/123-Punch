using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlaySoundExit : StateMachineBehaviour
{
    [SerializeField] private SoundType sound;
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManager.PlaySound(sound);
    }
}
