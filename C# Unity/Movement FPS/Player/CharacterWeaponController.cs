using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterWeaponController : MonoBehaviour
{
    private InputManager inputManager;

    [Header("Transforms")]
    public Transform cam;
    public Transform rightHeldLocation;
    public Transform leftHeldLocation;

    [Header("Equipment")]
    public List<GameObject> equippedWeaponsPrefabs = new List<GameObject>();


    [Header("Private")]
    [SerializeField] private GameObject _rightWeapon;
    [SerializeField] private WeaponController _rightWeaponController;
    [SerializeField] private GameObject _leftWeapon;
    [SerializeField] private WeaponController _leftWeaponController;

    private ActorStates _actorStates;

    void Start()
    {
        _actorStates = GetComponentInParent<ActorStates>();
        inputManager = GameObject.FindObjectOfType<InputManager>();
        Equip(0, true);
    }

    private void Update()
    {
        if (_actorStates.canAttack && CursorController.cursorLocked && !CursorController.justLocked)
        {
            if (inputManager.inputActions.Firing.Fire1.triggered)
            {
                _rightWeaponController.Shoot();
            }
            if (inputManager.inputActions.Firing.Fire2.triggered)
            {
                _leftWeaponController.Shoot();
            }
        }
    }

    public void Equip(int slot, bool rightWeapon)
    {
        if (rightWeapon)
        {
            GameObject weapon = Instantiate(equippedWeaponsPrefabs[slot], rightHeldLocation, true);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            _rightWeapon = weapon;
            _rightWeaponController = weapon.GetComponent<WeaponController>();
        }
    }

    public void Unequip(bool rightWeapon)
    {
        if (rightWeapon)
        {
            _rightWeaponController.Unequip();
            _rightWeapon = null;
            _rightWeaponController = null;
        }
    }

    public void Move()
    {
        transform.position = cam.transform.position;
        Vector3 rot = cam.transform.eulerAngles;
        rot.z = 0f;
        transform.rotation = Quaternion.Euler(rot); 
    }
}
