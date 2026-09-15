using UnityEngine;

public class PlayerRefrenceProvider : Singleton<PlayerRefrenceProvider> {

    [field:SerializeField] public CharacterController PlayerCharacterController { get; private set; }
    public Transform PlayerTransform => PlayerCharacterController.transform;
    [field:SerializeField] public Transform PlayerHeadTransform {  get; private set; }
    [field: SerializeField] public Transform PlayerPhoneToEarPosition { get; private set; }

}
