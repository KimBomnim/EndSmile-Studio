using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerDamage : MonoBehaviour
{
    [Header("UI")]
    public Image HpBar;                 // 체력바
    public Text HpText;                 // 체력 텍스트
    public GameObject deathPanel;

    [Header("Data")]
    public int curHp;                   // 현재 체력
    public bool isDie;                  // 사망 확인
    public GameObject hitEffect;        // 피격 이펙트
    public GameObject damageUIPrefab;   // 데미지 UI
    public GameObject restoreEffect;
 
    // 컴포넌트
    Animator animator;
    AudioSource source;
    // 스크립트
    ThirdPersonCtrl controller;
    PlayerAttack attack;
    PlayerState playerState;
    ChangeForm changeForm;

    // readonly
    readonly string M_AttackTag = "M_ATTACK";   // 몬스터 공격 콜라이더 태그
    readonly int hashDie = Animator.StringToHash("Die");
    readonly int hashHit = Animator.StringToHash("Hit");
    readonly int hashSpeed = Animator.StringToHash("Speed");

    readonly string bossAttackTag = "BossAttack"; // 보스 공격 태그
    readonly string greatSword = "GreatSword";

    // 사운드
    AudioClip hitSound;
    AudioClip deathSound;    

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<ThirdPersonCtrl>();
        attack = GetComponent<PlayerAttack>();
        playerState = GetComponent<PlayerState>();
        changeForm = GetComponent<ChangeForm>();
        source = GetComponent<AudioSource>();

        hitSound = Resources.Load<AudioClip>("Sound/Player/PlayerHit");
        deathSound = Resources.Load<AudioClip>("Sound/Player/PlayerDie");
        damageUIPrefab = Resources.Load<GameObject>("Effects/DamagePopUp");
    }

    void Start()
    {
        // 데이터가 없다면 체력 상태는 초기화 됨
        //if(데이터가 존재하지 않는다면)
        
        LoadCharacterData();
        hpUpdate();
    }

    // 캐릭터 초기 데이터(데이터 저장된것이 없는 경우 값)
    void LoadCharacterData()
    {
        curHp = PlayerStat.instance.maxHP;
    }

    // 체력 업데이트 함수(체력값이 변경될 때 마다 호출해야함)
    public void hpUpdate()
    {
        HpBar.fillAmount = (float)curHp / PlayerStat.instance.maxHP;                   // 체력바 이미지 수정
        HpText.text = curHp.ToString() + " / " + PlayerStat.instance.maxHP.ToString(); // 체력바 텍스트 수정

        // 체력이 30퍼이하인 경우 빨간색
        if (HpBar.fillAmount <= 0.3f)
            HpBar.color = Color.red;
        //체력이 50퍼이하인 경우 노란색
        else if (HpBar.fillAmount <= 0.5f)
            HpBar.color = Color.yellow;
        else HpBar.color = Color.green;
    }

    void Update()
    {
        //// 체력 감소 테스트 및 회복을 위한 테스트용 함수
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    curHp -= 10;
        //    animator.SetTrigger(hashHit);
        //    animator.SetFloat(hashSpeed, 0f);
        //    hpUpdate();
        //    playerState.state = PlayerState.State.HIT;
        //    if (curHp <= 0)
        //        StartCoroutine(Die());
        //}
            
    }

    // 피격 코루틴
    IEnumerator Hit(GameObject Enemy)
    {
        // Move, Idle인 경우에만 피격 모션 발생하도록 수정 예쩡
        // 현재 캐릭터가 공격중이 아니라면 애니메이션 실행
        if(playerState.state != PlayerState.State.ATTACK)
        {
            animator.SetTrigger(hashHit); // 피격 애니메이션 재생
            animator.SetFloat(hashSpeed, 0f); // 피격 애니메이션 실행시 움직이지 않도록 멈춤
        }

        // 에너미AI로부터 데미지 값을 받아옴(+ 랜덤하게 0~9사이 데미지 추가)
        int _damage = 0;
        
        if(Enemy.CompareTag("ENEMY"))
            _damage = (int)(Enemy.GetComponent<MonsterAI>().damage + Random.Range(0f, 9f)) -PlayerStat.instance.def;
        else if(Enemy.CompareTag(greatSword) || Enemy.CompareTag("Magic"))
            _damage = 50 - PlayerStat.instance.def;
        _damage = Mathf.Clamp(_damage, 0, 9999);

        curHp -= _damage;           // 현재 체력을 데미지 만큼 감소
        ShowDamageEffect(_damage);  // 데미지 이펙트 출력
        source.PlayOneShot(hitSound, 1.5f);

        // 현재 체력값이 0 ~ 초기 체력(아마 최대체력으로 변경될 예정)사이의 값만 가지도록 조정
        curHp = Mathf.Clamp(curHp, 0, PlayerStat.instance.maxHP);
        hpUpdate();

        playerState.state = PlayerState.State.HIT;  // 플레이어 상태->피격상태 로 변경
        
        // 피격 이펙트 생성(차후에 피격받은 지점을 구해 그곳에서 피격 이펙트 생성되도록 수정)
        GameObject hitEff = Instantiate(hitEffect, transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
        //Destroy(hitEff, 1.5f);
        yield return new WaitForSeconds(1.5f);
        Destroy(hitEff);
      
        //yield return new WaitForSeconds(0.4f);  
        //playerState.state = PlayerState.State.IDLE;
    }

    IEnumerator Die()
    {
        playerState.state = PlayerState.State.DIE;  // 사망 상태로 변경
        isDie = true;
       
        animator.SetTrigger(hashDie);     // 사망 애니메이션 실행
        GetComponent<CharacterController>().enabled = false;    // 충돌판정 제거를 위한 캐릭터 컨트롤러 비활성화
        source.PlayOneShot(deathSound, 1.8f);

        yield return null;
    }
    public void Respawn()
    {
        // 스폰 포인트 오브젝트 탐색
        Transform SpawnPoint = GameObject.Find("SpawnManager").
            transform.GetChild(3).GetComponent<Transform>();
        // 스폰 포인트에 리스폰
        transform.position = SpawnPoint.position;
        transform.rotation = Quaternion.Euler(0,  -90, 0);
        // 체력은 꽉 찬 상태로 리스폰
        curHp = PlayerStat.instance.maxHP;
        HpBar.color = Color.green;
        hpUpdate();
        // 플레이어 상태 = IDLE
        playerState.state = PlayerState.State.IDLE;
        // 각종 컴포넌트 활성화(부활시 여우 상태로 부활)
        GetComponent<CharacterController>().enabled = true;
        changeForm.RespawnToForm(); // UI 패널 여우로 변경
        isDie = false;  // 사망상태 해제

        StandardInput cursorLock = GetComponent<StandardInput>();
        cursorLock.move = Vector2.zero;
        cursorLock.look = Vector2.zero;
        cursorLock.cursorLocked = true;
        cursorLock.cursorInputForLook = true;
        //UIManager.instance.deathPanel.SetActive(false);

        // onEnable함수를 불러오기 위한 작업
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    private void OnEnable()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        // 몬스터에게 공격을 맞았다면
        if(other.CompareTag(M_AttackTag) || other.CompareTag(greatSword))
        {
            // 죽었거나 피격 상태 라면 실행하지 않음
            if (isDie ||
            playerState.state == PlayerState.State.DIE ||
            playerState.state == PlayerState.State.HIT)
                return;
            // 에너미 정보를 넘김(데미지 값만큼 체력이 달아야하기 때문)
            GameObject EnemyInfo = null;
            if (other.CompareTag(M_AttackTag))
            {
                EnemyInfo = other.GetComponentInParent<MonsterAI>().gameObject;
            }
            if (other.CompareTag(greatSword))
            {
                EnemyInfo = other.gameObject;
            }
            if (other.CompareTag("Magic"))
            {
                EnemyInfo = other.gameObject;
            }

            StartCoroutine(Hit(EnemyInfo));
            // 체력이 0이하가 되면 Die코루틴 실행
            if (curHp <= 0)
                StartCoroutine("Die");
        }
    }

    void ShowDamageEffect(int _damage) // 플레이어 위치는 transform이 되므로 위치 정보를 받을 필요 없음
    {
        Vector3 MonsterHeader = new Vector3(transform.position.x + Random.Range(-0.5f, 0.5f),
                                            transform.position.y + 1.8f,
                                            transform.position.z + Random.Range(-0.5f, 0.5f));
        GameObject Effect_M_DamageAmount = Instantiate(damageUIPrefab,
                                           MonsterHeader,
                                           Quaternion.identity,
                                           transform);
        Effect_M_DamageAmount.GetComponent<TextMeshPro>().text = _damage.ToString();
        Effect_M_DamageAmount.GetComponent<TextMeshPro>().color = new Color(150f, 0f, 255f);
    }

    void OnHit()    // 피격 이벤트
    {
        animator.SetFloat(hashSpeed, 0f);
    }

    void OnHitEnd()
    {
        if(!isDie)
            playerState.state = PlayerState.State.IDLE;
    }

    // 체력 회복 함수(포션 사용, 힐 스킬 사용)
    public void RestoreHp(int newHp)
    {
        // 죽었으면 함수 종료
        if (playerState.state == PlayerState.State.DIE)
            return;
        curHp += newHp;     // newHP만큼 체력 회복
        // 체력된 회복이 최대체력보다 큰 경우 조정
        if (curHp > PlayerStat.instance.maxHP)
            curHp = PlayerStat.instance.maxHP;
        hpUpdate();
    }

    public void UsePotionEffect()
    {
        GameObject Effect = Instantiate(restoreEffect, transform.position, Quaternion.identity);
        Destroy(Effect, 1f);
    }
}
