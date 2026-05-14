using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    private SPUM_Prefabs _spum;

    public float speed = 5f;
    public float attackRange = 1.5f;
    public Transform target;

    public float dashDistance = 3f;
    public float dashSpeed = 20f;
    public float dashCooldown = 1f;

    public int attackIndex = 0;
    public int skillIndex = 1;
    public float skillCooldown = 3f;

    public GameObject attackEffect;
    public GameObject skillEffect;
    public GameObject dashEffect;

    public Transform attackFirePoint;
    public Transform skillFirePoint;

    public float effectSpeed = 10f;
    public float effectRange = 5f;

    public float attackEffectSize = 1f;
    public float skillEffectSize = 2f;
    public float dashEffectSize = 1f;

    public bool attackEffectProjectile = false;
    public bool skillEffectProjectile = false;
    public bool dashEffectProjectile = false;

    public bool flipEffect = false;

    private Vector3 lastMouseWorldPos;
    private bool isDashing = false;
    private float lastDashTime = -999f;
    private float lastSkillTime = -999f;

    private enum State { Idle, Moving, Attacking, Dashing, Skill }
    private State _currentState = State.Idle;

    void Start()
    {
        _spum = GetComponent<SPUM_Prefabs>();
        _spum.OverrideControllerInit();
        _spum.PlayAnimation(PlayerState.IDLE, 0);
    }

    void Update()
    {
        // 매 프레임 마우스 월드 위치 저장
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        lastMouseWorldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(mouseScreenPos.x, mouseScreenPos.y, 10f)
        );
        lastMouseWorldPos.z = 0;

        // 스페이스바 → 대시
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (Time.time - lastDashTime >= dashCooldown && !isDashing)
                StartCoroutine(Dash());
        }

        // Q키 → 스킬
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            if (Time.time - lastSkillTime >= skillCooldown
                && _currentState != State.Skill
                && _currentState != State.Dashing)
            {
                StopAllCoroutines();
                StartCoroutine(UseSkill());
            }
        }

        if (isDashing) return;
        if (_currentState == State.Skill) return;

        // 좌클릭 → 공격
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (_currentState != State.Attacking)
            {
                _currentState = State.Attacking;
                StartCoroutine(AttackOnce());
            }
        }

        // WASD → 이동
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.wKey.isPressed) moveY = 1f;
        if (Keyboard.current.sKey.isPressed) moveY = -1f;
        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = 1f;

        Vector3 moveDir = new Vector3(moveX, moveY, 0).normalized;

        if (_currentState != State.Attacking)
        {
            if (moveDir != Vector3.zero)
            {
                _currentState = State.Moving;
                FlipSprite(moveX);
                _spum.PlayAnimation(PlayerState.MOVE, 0);
                transform.position += moveDir * speed * Time.deltaTime;
            }
            else
            {
                if (_currentState == State.Moving)
                {
                    _currentState = State.Idle;
                    _spum.PlayAnimation(PlayerState.IDLE, 0);
                }
            }
        }
    }

    IEnumerator AttackOnce()
    {
        float dirX = lastMouseWorldPos.x - transform.position.x;
        FlipSprite(dirX);

        _spum.PlayAnimation(PlayerState.ATTACK, attackIndex);
        SpawnEffect(attackEffect, attackFirePoint, attackEffectSize, attackEffectProjectile);
        yield return new WaitForSeconds(1.0f);
        _currentState = State.Idle;
        _spum.PlayAnimation(PlayerState.IDLE, 0);
    }

    IEnumerator UseSkill()
    {
        _currentState = State.Skill;
        lastSkillTime = Time.time;

        float dirX = lastMouseWorldPos.x - transform.position.x;
        FlipSprite(dirX);

        _spum.PlayAnimation(PlayerState.ATTACK, skillIndex);
        SpawnEffect(skillEffect, skillFirePoint, skillEffectSize, skillEffectProjectile);
        yield return new WaitForSeconds(1.0f);
        _currentState = State.Idle;
        _spum.PlayAnimation(PlayerState.IDLE, 0);
    }

    IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;
        _currentState = State.Dashing;

        Vector3 dashDir = (lastMouseWorldPos - transform.position).normalized;
        Vector3 dashTarget = transform.position + dashDir * dashDistance;
        dashTarget.z = 0;

        FlipSprite(dashDir.x);
        _spum.PlayAnimation(PlayerState.MOVE, 0);
        SpawnEffect(dashEffect, attackFirePoint, dashEffectSize, dashEffectProjectile);

        while (Vector3.Distance(transform.position, dashTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                dashTarget,
                dashSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = dashTarget;
        isDashing = false;
        _currentState = State.Idle;
        _spum.PlayAnimation(PlayerState.IDLE, 0);
    }

    void SpawnEffect(GameObject effectPrefab, Transform firePoint, float size = 1f, bool isProjectile = false)
    {
        if (effectPrefab == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        spawnPos.z = 0;

        Quaternion rotation;
        Vector3 dir;

        if (isProjectile)
        {
            dir = (lastMouseWorldPos - spawnPos).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            float facing = _spum.transform.localScale.x;
            dir = facing > 0 ? Vector3.right : Vector3.left;
            float angle = facing > 0 ? 0f : 180f;
            rotation = Quaternion.Euler(0, 0, angle);
        }

        GameObject effect = Instantiate(effectPrefab, spawnPos, rotation);

        float facingScale = flipEffect ? _spum.transform.localScale.x : 1f;
        effect.transform.localScale = new Vector3(size * facingScale, size, size);

        if (isProjectile)
        {
            EffectMover mover = effect.AddComponent<EffectMover>();
            mover.direction = dir;
            mover.speed = effectSpeed;
            mover.lifetime = 3f;
            mover.maxDistance = effectRange;
        }
        else
        {
            Destroy(effect, 1f);
        }
    }

    void FlipSprite(float directionX)
    {
        if (directionX < 0)
            _spum.transform.localScale = new Vector3(1, 1, 1);
        else
            _spum.transform.localScale = new Vector3(-1, 1, 1);
    }

    public void Die()
    {
        _currentState = State.Idle;
        StopAllCoroutines();
        _spum.PlayAnimation(PlayerState.DEATH, 0);
    }
}