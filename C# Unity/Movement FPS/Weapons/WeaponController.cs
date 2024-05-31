using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class WeaponController : MonoBehaviour
{
    public enum FireType { Hitscan, Projectile }
    [Header("Types")]
    public FireType fireType = FireType.Hitscan;

    // if projectile show this - give prefab
    [HideInInspector] public GameObject projectile;

    [Header("")]
    public Transform fireOriginTransform;
    public LayerMask hitLayers;

    [Header("Stats")]
    public float damage = 50f;
    private bool _fireReady = true;
    public float fireRate = 4f;
    private float _fireTimer = 0f;
    public float maxFireDistance = Mathf.Infinity;

    public bool infiniteClip = true;
    [HideInInspector] public int clipSize = 8;
    private int clipAmount = 0;

    [Header("Animation Speed")]
    public float fireAnimSpeed = 1f;
    public float reloadAnimSpeed = 1f;

    protected GameObject _cam;

    protected Animator _animator;

    // Start is called before the first frame update
    protected void Start()
    {
        _cam = GameObject.FindGameObjectWithTag("MainCamera");
        _animator = GetComponent<Animator>();
        EndReload();
        Equip();
    }

    protected void Update()
    {
        if (!_fireReady)
        {
            _fireTimer += Time.deltaTime;
            if (_fireTimer >= 1 / fireRate)
                _fireReady = true;
        }
    }

    public void Shoot()
    {
        if (_fireReady)
        {
            _fireReady = false;
            _fireTimer = 0;
            //Shoot animation calls Fire()
            _animator.Play("Shoot", 0, 0f);
        }
    }

    protected void Fire()
    {
        if (fireType == FireType.Hitscan)
        {
            Vector3 fireOriginPos = fireOriginTransform.position;
            Ray ray = new Ray(fireOriginPos, fireOriginTransform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxFireDistance, hitLayers, QueryTriggerInteraction.Ignore))
            {
                GameObject hitObj = hit.collider.gameObject;

                if (hitObj.CompareTag("Actor"))
                {
                    DamageInstance di = new DamageInstance(gameObject, damage);
                    hitObj.GetComponentInParent<ActorHealth>().Damage(di);
                }
            }
        }
        else if (fireType == FireType.Projectile)
        {
            GameObject proj = Instantiate(projectile, fireOriginTransform.position, fireOriginTransform.rotation);
            proj.SetActive(true);
        }
    }

    public void StartReload()
    {
        _animator.Play("Reload");
    }

    protected void EndReload()
    {
        clipAmount = clipSize;
    }

    public void Equip()
    {
        _animator.Play("Equip");
    }

    public void Unequip()
    {
        _animator.Play("Unequip");
    }

    public void EndUnequip()
    {
        Destroy(gameObject);
    }
}

