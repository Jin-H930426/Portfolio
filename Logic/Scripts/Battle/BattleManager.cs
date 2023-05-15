using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using JH.Portfolio.Animation;
using JH.Portfolio.Character;
using JH.Portfolio.Item;
using JH.Portfolio.Manager;
using JH.Portfolio.Map;
using Unity.VisualScripting;
using UnityEngine;

namespace JH.Portfolio.Battle
{
    public class BattleManager : MonoBehaviour
    {
        // 배틀 상태 텍스트 정의
        private const string BATTlE_START = "Battle Start";
        private const string YOU_DIE = "You Die";
        private const string YOU_WIN = "You Win";

        private const int RANGEALLBITMASK = (int)SkillRange.TeamAll | (int)SkillRange.EnemyAll;
        private const int ALLYBITMASK = (int)SkillRange.Self | (int)SkillRange.TeamAll | (int)SkillRange.TeamUnit;
        private const int TARGETBITMASK = (int)SkillRange.TeamUnit | (int)SkillRange.EnemyUnit;

        // command 정의
        private const int PLAYER_PICK_COMMAND = 0; // 플레이어 선택
        private const int ACTION_PICK_COMMAND = 1; // 행동 선택
        private const int SKILL_PICK_COMMAND = 2; // 스킬 선택
        private const int ITEM_PICK_COMMAND = 3; // 아이템 선택
        private const int TARGET_PICK_COMMAND = 4; // 타겟 선택
        private const int NONE_USE_PICK_COMMAND = 5; // 사용하지 않는 command

        // 배틀에서 사용될 wait 정의
        WaitForSeconds waitOneSeconds = new WaitForSeconds(1f);
        private WaitWhile waitCommandInput;

        // command 상태에 따라 실행될 event list
        private (Action<Vector3> move, Action selection, Action cancel)[] _commandActions;

        // 현재 command 상태
        private int _COMMAND_INDEX = NONE_USE_PICK_COMMAND;

        // 이전 command 상태
        private int PREVCOMMAND_INDEX = ACTION_PICK_COMMAND;

        /// <summary>
        /// Command 상태를 변경하면서 이전 command 상태를 저장하는 프로퍼티
        /// </summary>
        private int COMMAND_INDEX
        {
            get => _COMMAND_INDEX;
            set
            {
                if (COMMAND_INDEX != NONE_USE_PICK_COMMAND)
                {
                    PREVCOMMAND_INDEX = _COMMAND_INDEX;
                }

                _COMMAND_INDEX = value;
            }
        }


        [Header("Unit Property")] public BattleUnit[] playerUnits;
        public BattleUnit[] enemyUnits;
        public BattleAI ai;
        public bool isSingleMode = true;
        public int playerUnitCount = 1;
        public int enemyUnitCount = 1;
        public int playerDeadCount = 0;
        public int enemyDeadCount = 0;

        [Header("Animation Property")] public Camera.CameraMixer mixer;
        public ScaleAnimation battleStateAnimation;
        [Range(0, 1), SerializeField] private float _height = 0;

        [Header("Battle Property")] public BattleUI ui;
        [Tooltip("선택된 플레이어를 표시하는 게임 오브젝트")] public Transform playablePicker;
        [Tooltip("선택된 몬스터를 표시하는 게임 오브젝트")] public Transform enemyPicker;

        private bool _useEnemyPicker; // enemyPicker의 display 상태

        private int _playerPickIndex = 0; // 선택된 플레이어의 인덱스
        private int _playableCommandIndex = 0; // 캐릭터가 선택할 커맨드의 인덱스
        private int _startViewSkillAndItemIndex = 0; // 스킬리스트에서 첫번째로 또는 스킬 및 아이템 인덱스
        private int _skillAndItemIndex = 0; // 선택된 스킬 또는 아이템 인덱스
        private int _targetPickIndex = 0; // 선택된 몬스터의 인덱스

        private bool _waitCommand = false; // 플레이어의 커맨드 선택이 끝난 상태인지 확인하기 위한 변수
        private Heap<BattleUnit> battleUnitHeap = new Heap<BattleUnit>(8); // 플레이어와 몬스터의 힙

        /// <summary>
        /// 커맨드 창의 현재 높이 상태
        /// 0: 커맨드 창이 내려간 상태
        /// 1: 커맨드 창이 올라온 상태
        /// </summary>
        public float height
        {
            get => _height;
            set
            {
                ui.height = value;
                mixer.MixWeight = value;
                _height = value;
            }
        }

        public int SkillAndItemCount(int command) => command == SKILL_PICK_COMMAND
            ? playerUnits[_playerPickIndex].SkillCount
            : Inventory.GetItemCount();


        public int GetTargetCount(bool ally) => ally ? playerUnitCount : enemyUnitCount;


        #region Initialize

        private void Start()
        {
            if (ui == null)
            {
                ui = FindObjectOfType<BattleUI>();
                if (ui == null)
                {
                    Debug.LogError("Don't find Battle UI");
                }
            }

            waitCommandInput = new WaitWhile(() => _waitCommand);
            UnitSet();
            CharacterInterfaceSet();
            CommandSet();

            StopCoroutine("BattleCoroutine");
            StartCoroutine("BattleCoroutine");
        }

        private void OnDisable()
        {
            CommandRelease();
        }

        void UnitSet()
        {
            for (var i = 0; i < playerUnits.Length; i++)
            {
                playerUnits[i].SetActive(i < playerUnitCount);
            }

            for (var i = 0; i < enemyUnits.Length; i++)
            {
                enemyUnits[i].SetActive(i < enemyUnitCount);
            }
        }

        void CharacterInterfaceSet()
        {
            for (var i = 0; i < playerUnits.Length; i++)
            {
                ui.SetCharecterCard(i, i < playerUnitCount ? playerUnits[i].GetStatus() : "");
            }
        }

        /// <summary>
        /// 커맨드 설정
        /// </summary>
        void CommandSet()
        {
            // 커맨드 이벤트 리스트를 설정해줌
            _commandActions = new[]
            {
                // 캐릭터 선택 커맨드 상태 이벤트
                ((Action<Vector3>, Action, Action ))(
                    PlayerPickCommandMove, // move input event 
                    PlayerPickCommandSelection,
                    PlayerPickCommandCancel),
                // 캐릭터 동작 커맨드 상태 이벤트
                ((Action<Vector3> move, Action select, Action cancle ))(
                    ActionCommandMove, // move input event
                    ActionCommandSelection, // Select
                    ActionCommandCancel),
                // 스킬 선택 커맨드 상태 이벤트
                ((Action<Vector3>, Action, Action ))(
                    SkillAndItemMove, // move input event
                    SkillAndItemSelection, // Select 
                    SkillAndItemCancel), // Cancel 
                // 아이템 선택 커맨드 상태 이벤트
                ((Action<Vector3>, Action, Action ))(
                    SkillAndItemMove, // move input event
                    SkillAndItemSelection, // Select 
                    SkillAndItemCancel), // Cancel 
                // 타겟 선택 커맨드 상태 이벤트
                ((Action<Vector3>, Action, Action ))(
                    TargetPickerMove, // move input event
                    TargetPickerSelection, // Select 
                    TargetPickerCancel), // Cancel 
                // 커맨드 사용 불가 상태
                ((Action<Vector3>, Action, Action ))( // None use Pick
                    (input) => { }, // move input event
                    () => { }, // Select 
                    () => { }), // Cancel
            };

            GameManager.InputManager.OnMoveInputEvent.OnPressedEvent += CallMoveEvent;
            GameManager.InputManager.OnAttackInputEvent.OnPressedEvent += CallSelectionEvent;
            GameManager.InputManager.OnReloadInputEvent.OnPressedEvent += CallCancelEvent;
        }

        /// <summary>
        /// 커맨드 상태 해제
        /// </summary>
        void CommandRelease()
        {
            // 이벤트를 해제해줌
            _commandActions = null;
            // inputManager가 없는 경우 예외처리
            if (GameManager.InputManager == null) return;
            GameManager.InputManager.OnMoveInputEvent.OnPressedEvent -= CallMoveEvent;
            GameManager.InputManager.OnAttackInputEvent.OnPressedEvent -= CallSelectionEvent;
            GameManager.InputManager.OnReloadInputEvent.OnPressedEvent -= CallCancelEvent;
        }

        private void CallMoveEvent(Vector3 input)
        {
            _commandActions[COMMAND_INDEX].move(input);
        }

        private void CallSelectionEvent()
        {
            _commandActions[COMMAND_INDEX].selection();
        }

        private void CallCancelEvent()
        {
            _commandActions[COMMAND_INDEX].cancel();
        }

        #endregion

        #region Battle logic

        private IEnumerator BattleCoroutine()
        {
            // 배틀이 시작되면 실행되는 애니메이션이 종료될 때까지 대기
            yield return battleStateAnimation.Play();
            battleStateAnimation.gameObject.SetActive(false);
            // 애니메이션 종료후 잠시 대기
            yield return waitOneSeconds;

            while (true)
            {
                // 커맨드 상태를 입력 불가 상태로 변경
                COMMAND_INDEX = NONE_USE_PICK_COMMAND;
                // 게임의 종료 상태를 확인함 아군 전멸 또는 적군 전멸 시 true
                // 모든 캐릭터의 죽음 여부확인
                // 버프 지속시간, 쿨타임 시간을 확인함
                if (GameEndCheck()) break;


                // 입력을 확인 위한 커맨드 창을 위로 올려줌                
                yield return SetCommandBoxHeight_Coroutine(1);

                // 플레이어 턴을 시작함
                PlayerTurn();
                // 플레이어의 입력을 
                yield return waitCommandInput;
                // 커맨드 상태를 입력 불가 상태로 전환
                COMMAND_INDEX = NONE_USE_PICK_COMMAND;
                // 적군 턴을 시작함
                EnemyTurn();
                // 적군 입력 대기
                yield return waitCommandInput;
                // 커맨드 창을 아래로 내림
                yield return SetCommandBoxHeight_Coroutine(-1);
                // 캐릭터의 우선 순위에 맞춰 정렬된 힙을 이용해 턴을 진행함
                while (battleUnitHeap.Count > 0)
                {
                    yield return battleUnitHeap.Pop().Command();
                    CharacterInterfaceSet();
                }
            }

            // 배틀이 종료되면 종료 애니메이션을 실행함
            yield return battleStateAnimation.Play();
            BattleExit();
        }

        bool GameEndCheck()
        {
            // 아군과 적군의 deathCount를 확인함
            var pDeadCount = 0;
            var eDeadCount = 0;
            foreach (var playerUnit in playerUnits)
            {
                if (playerUnit.isDead) pDeadCount++;
                playerUnit.EndTurn();
            }

            foreach (var enemyUnit in enemyUnits)
            {
                if (enemyUnit.isDead) eDeadCount++;
                enemyUnit.EndTurn();
            }

            // 죽은 캐릭터의 수를 저장함
            playerDeadCount = pDeadCount;
            enemyDeadCount = eDeadCount;

            // 게임이 종료되는 상황이면 종료
            if (enemyDeadCount == enemyUnitCount) // 모든 적군이 죽은 경우
            {
                battleStateAnimation.gameObject.SetActive(true);
                battleStateAnimation.GetComponent<UnityEngine.UI.Text>().text = YOU_WIN;
                // 살아 남은 플레이어만 경험치를 나눠가짐
                int expSum = 0;
                for (var i = 0; i < enemyUnitCount; i++)
                {
                    expSum += enemyUnits[i].GetExp();
                }

                int unitExp = expSum / (playerUnitCount - playerDeadCount);
                // 경험치를 나눠가짐
                for (var i = 0; i < playerUnitCount; i++)
                {
                    if (!playerUnits[i].isDead)
                        playerUnits[i].AddExp(unitExp); // 경험치 증가
                }

                return true;
            }

            if (playerDeadCount == playerUnitCount) // 모든 플레이어가 죽은 경우
            {
                battleStateAnimation.gameObject.SetActive(true);
                battleStateAnimation.GetComponent<UnityEngine.UI.Text>().text = YOU_DIE;
                // 패널티를 부과함
                foreach (var unit in playerUnits)
                {
                    unit.AddExp(-10); //경험치 감소
                }

                return true;
            }

            return false;
        }

        void PlayerTurn()
        {
            // 플레이어의 턴을 시작함
            _waitCommand = true;
            // 플레이어의 턴을 시작할 때 플레이어의 첫번째 유닛을 선택함
            _playerPickIndex = 0;
            // 플레이어의 첫번째 유닛이 죽었으면 다음 살아있는 유닛을 선택함
            if (playerUnits[_playerPickIndex].isDead) AddPlayerPickIndex(1);
            // 행동 선택 0번째로 focus 초기화
            _playableCommandIndex = 0;
            // 입력 커맨드를 행동 선택 커맨드로 변경
            COMMAND_INDEX = ACTION_PICK_COMMAND;

            SetPicker(true, playablePicker, playerUnits[_playerPickIndex].transform.position + Vector3.up);
            ui.PickTarget(PLAYER_PICK_COMMAND, ui.charecterCards[_playerPickIndex]);
            ui.PickTarget(COMMAND_INDEX, ui.commandCards[_playableCommandIndex]);
        }

        void EnemyTurn()
        {
            // 적군 턴을 시작 함
            _waitCommand = true;
            // 만약 싱글플레이일 경우 ai를 통해 적군의 행동을 설정함
            if (isSingleMode)
            {
                ai.Init(this, false);
                ai.AI();
                // ai의 설정이 끝나면 적군의 턴을 종료함
                _waitCommand = false;
            }
        }

        void BattleExit()
        {
        }


        /// <summary>
        /// 캐릭터의 동작을 입력 시켜줌
        /// </summary>
        /// <param name="skillData">스킬 정보</param>
        /// <param name="unit">유닛 정보</param>
        /// <param name="isItem">아이템 사용 여부</param>
        /// <param name="target">타겟 정보</param>
        public void BattleCommandSetting(SkillData skillData,
            BattleUnit unit, bool isItem, params BattleUnit[] target)
        {
            unit.UnitCommandEnter(skillData, isItem, target);
            battleUnitHeap.Push(unit);
        }

        #endregion

        #region Pick Control

        int AddPlayerPickIndex(int add)
        {
            int value = _playerPickIndex + add;
            // down overflow
            if (value < 0) return -1;
            // up overflow
            if (value >= playerUnitCount) return 1;

            while (playerUnits[value].isDead)
            {
                value += add;
                if (value < 0) return -1;
                if (value >= playerUnitCount) return 1;
            }

            _playerPickIndex = value;
            return 0;
        }

        public void AddTargetPickIndex(int add, bool ally)
        {
            var value = _targetPickIndex + add;
            var targetCount = GetTargetCount(ally);

            _targetPickIndex = (value + targetCount) % targetCount;
            while ((ally ? playerUnits[_targetPickIndex] : enemyUnits[_targetPickIndex]).isDead)
            {
                _targetPickIndex = (_targetPickIndex + add + targetCount) % targetCount;

                if (_targetPickIndex == value)
                {
                    break;
                }
            }
        }

        public void AddPlayableCommandIndex(int add)
        {
            var value = add + _playableCommandIndex;
            _playableCommandIndex = (value + 4) % 4;

            ui.PickTarget(COMMAND_INDEX, ui.commandCards[_playableCommandIndex]);
        }

        public void SetStartViewSkillAndItemIndex(int value, int command)
        {
            var count = SkillAndItemCount(command);

            var index = Mathf.Clamp(value, 0, Mathf.Max(0, count - 4));
            _startViewSkillAndItemIndex = index;

            for (int i = 0; i < 4; i++)
            {
                var nowIndex = index + i;
                if (nowIndex >= count)
                {
                    ui.SetSkillAndItemCards(i, null, "", "");
                    continue;
                }

                (Sprite icon, string name, string description) data = command == SKILL_PICK_COMMAND
                    ? playerUnits[_playerPickIndex].GetSkillDisplayInfo(nowIndex)
                    : Inventory.GetItemDisplayInfo(nowIndex);
                ui.SetSkillAndItemCards(i, data.icon, data.name, data.description);
            }
        }

        public void AddSkillAndItemIndex(int add, int command)
        {
            var value = _skillAndItemIndex + add;

            _skillAndItemIndex = Mathf.Clamp(value, 0, Mathf.Max(0, SkillAndItemCount(command) - 1));
            if (_startViewSkillAndItemIndex >= value) SetStartViewSkillAndItemIndex(value, command);
            else if (value >= _startViewSkillAndItemIndex + 4) SetStartViewSkillAndItemIndex(value - 3, command);
        }

        void SetPicker(bool state, Transform picker, Vector3 targetPos)
        {
            picker.gameObject.SetActive(state);
            if (!state) return;
            picker.position = targetPos;
        }

        #endregion

        #region Command Action

        void PlayerPickCommandMove(Vector3 inputDir)
        {
        }

        void PlayerPickCommandSelection()
        {
        }

        void PlayerPickCommandCancel()
        {
        }

        /// <summary>
        /// inputManager의 입력에 따른 행동 커맨드 (이동)
        /// </summary>
        /// <param name="inputDir">input manager의 입력 방향</param>
        void ActionCommandMove(Vector3 inputDir)
        {
            // 행동 커맨드의 선택을 이동시킴
            AddPlayableCommandIndex(Mathf.RoundToInt(inputDir.x) -
                                    (Mathf.RoundToInt(inputDir.z) * 2));
        }

        /// <summary>
        /// inputManager의 입력에 따른 행동 커맨드 (선택)
        /// </summary>
        void ActionCommandSelection()
        {
            switch (_playableCommandIndex)
            {
                case 0: // 공격
                {
                    // 0번째 스킬을 선택함
                    _skillAndItemIndex = 0;
                    // 스킬 커맨드 창을 올린 후 현재 커맨드 상태를 스킬 선택으로 변경
                    StartCoroutine("SkillAndItemCommandOn", SKILL_PICK_COMMAND);
                    break;
                }
                case 1: // 방어
                {
                    playerUnits[_playerPickIndex].DefenseCommand();
                    // 다음 캐릭터로 이동
                    SelectNextCharacter();
                    break;
                }
                case 2: // 아이템 사용
                {
                    // 0번째 아이템을 선택함
                    _skillAndItemIndex = 0;
                    // 아이템 커맨드 창을 올린 후 현재 커맨드 상태를 아이템 선택으로 변경
                    StartCoroutine("SkillAndItemCommandOn", ITEM_PICK_COMMAND);
                    break;
                }
                case 3: // 도망
                {
                    // 배틀을 종료함
                    BattleExit();
                    break;
                }
            }

            void SelectNextCharacter()
            {
                // 다음 플레이어가 없는  경우 1
                // 다음 플레이어가 존재 하는 경우 0
                // 이전 플레이어가 없는 경우 -1
                // 입력 값이 -1인 경우 반대
                int overFlow = AddPlayerPickIndex(1);
                if (overFlow == 1) // 모든 캐릭터의 선택이 종료됨
                {
                    // 턴을 종료 시킴
                    _waitCommand = false;
                    // 플레이어 선택 해제
                    SetPicker(false, playablePicker, Vector3.zero);
                    ui.RelaseTarget(PLAYER_PICK_COMMAND);
                    return;
                }
                else if (overFlow == 0) // 다음 캐릭터 선택으로 넘어감
                {
                    // 다음 플레이어로 픽커을 넘김
                    SetPicker(true, playablePicker,
                        playerUnits[_playerPickIndex].transform.position + Vector3.up);
                    ui.PickTarget(PLAYER_PICK_COMMAND,
                        ui.charecterCards[_playerPickIndex]);
                }
            }
        }

        /// <summary>
        /// inputManager의 입력에 따른 행동 커맨드 (취소) 
        /// </summary>
        void ActionCommandCancel()
        {
            // 이전의 캐릭터가 존재하는 경우
            if (AddPlayerPickIndex(-1) == 0)
            {
                // 이전 캐릭터로 픽커을 넘김
                ui.PickTarget(PLAYER_PICK_COMMAND, ui.charecterCards[_playerPickIndex]);
                SetPicker(true, playablePicker, playerUnits[_playerPickIndex].transform.position + Vector3.up);
            }
        }

        /// <summary>
        /// inputManager의 입력에 따른 스킬/아이템 커맨드 (이동)
        /// </summary>
        /// <param name="inputDir">입력 방향</param>
        void SkillAndItemMove(Vector3 inputDir)
        {
            int command = COMMAND_INDEX;
            // 피커의 위치를 이동 시킨다 (상하)
            AddSkillAndItemIndex(-Mathf.RoundToInt(inputDir.z), command);

            var index = _skillAndItemIndex - _startViewSkillAndItemIndex;
            ui.PickTarget(command, ui.skillAndItemCards[index].rectTransform);
        }

        /// <summary>
        /// inputManager의 입력에 따른 스킬/아이템 커맨드 (선택)
        /// </summary>
        void SkillAndItemSelection()
        {
            int command = COMMAND_INDEX;
            bool isItem = command == ITEM_PICK_COMMAND;

            // 행동할 캐릭터 선택
            var unit = playerUnits[_playerPickIndex];
            // 선택된 인덱스의 아이템 스킬/캐릭터 스킬을 가져옴
            var skill = isItem
                ?
                // 아이템 선택 커맨드일 경우 아이템의 스킬을 호출함 
                Inventory.GetItemSkillData(_skillAndItemIndex)
                :
                // 스킬 선택 커맨드일 경우 캐릭터의 스킬을 호출함 
                unit.GetSkillData(_skillAndItemIndex);

            if (unit.CoolDownCheck(skill)) return; // 스킬 쿨타임 확인
            if (!unit.CostCheck(skill)) return; // 스킬 사용에 필요한 코스트 비교


            SkillRange range = skill.range;

            // 타겟 팅이 가능한 경우
            bool target = ((int)range & TARGETBITMASK) != 0;
            // 아군을 타겟으로 하는 경우
            bool ally = ((int)range & ALLYBITMASK) != 0;
            // 아군, 적군 중 모든 유닛을 타깃으로 하는 경우
            bool all = ((int)range & RANGEALLBITMASK) != 0;

            ui.RelaseTarget(command);

            int nextCommand = target ? 1 : // 타겟팅이 필요한 경우
                !all ? 2 : // 자기 자신을 타겟 하는 경우
                ally ? 3 : // 아군 전체를 타겟으로 하는 경우
                4; // 적군 전체를 타겟으로 하는 경우

            switch (nextCommand)
            {
                case 1: // 타겟팅이 필요한 경우
                {
                    _targetPickIndex = 0;
                    // 선택할 적 캐릭터를 찾음
                    var ts = ally ? playerUnits[_targetPickIndex] : enemyUnits[_targetPickIndex];
                    if (ts.isDead)
                    {
                        AddTargetPickIndex(1, ally);
                        ts = ally ? playerUnits[_targetPickIndex] : enemyUnits[_targetPickIndex];
                    }

                    // 캐릭터를 선택하고 커맨드르 타겟 선택으로 변경
                    SetPicker(true, enemyPicker, ts.transform.position + Vector3.up);
                    StartCoroutine("SkillAndItemCommandDown", TARGET_PICK_COMMAND);

                    break;
                }
                case 2: // 자기 자신을 타겟팅 하는 경우
                {
                    BattleCommandSetting(skill, unit, isItem, unit);
                    SelectNextCharacter();
                    break;
                }
                case 3: // 아군 전체를 타겟 하는 경우
                {
                    var targetUnits = ally
                        ? playerUnits
                        : // 아군일 경우 아군 전체가 타겟
                        enemyUnits; // 적군일 경우 적군 전체가 타겟

                    // 설정된 데이터를 바탕으로 스킬을 사용함
                    BattleCommandSetting(skill, unit, isItem, targetUnits);
                    SelectNextCharacter();
                    break;
                }
                case 4: // 적군 전체를 타겟으로 하는 경우
                {
                    var targetUnits = ally
                        ? playerUnits
                        : // 아군일 경우 아군 전체가 타겟
                        enemyUnits; // 적군일 경우 적군 전체가 타겟

                    // 설정된 데이터를 바탕으로 스킬을 사용함
                    BattleCommandSetting(skill, unit, isItem, targetUnits);
                    SelectNextCharacter();
                    break;
                }
            }

            // 다음 캐릭터 선택으로 넘어가는 부분
            void SelectNextCharacter()
            {
                // 다음 선택할 캐릭터가 있는지 확인
                int overFlow = AddPlayerPickIndex(1);
                if (overFlow == 1) // 다음 캐릭터가 없으면 커맨드를 종료한다.
                {
                    _waitCommand = false;

                    SetPicker(false, playablePicker, Vector3.zero);
                    ui.RelaseTarget(PLAYER_PICK_COMMAND);
                    StartCoroutine("SkillAndItemCommandDown", NONE_USE_PICK_COMMAND);

                    return;
                }

                SetPicker(true, playablePicker,
                    playerUnits[_playerPickIndex].transform.position + Vector3.up);
                ui.PickTarget(PLAYER_PICK_COMMAND, ui.charecterCards[_playerPickIndex]);
                StartCoroutine("SkillAndItemCommandDown", ACTION_PICK_COMMAND);
            }
        }

        /// <summary>
        /// inputManager의 입력에 따른 스킬/아이템 커맨드 (취소)
        /// </summary>
        void SkillAndItemCancel()
        {
            // 커맨드 선택을 해제하고 행동 선택 커맨드로 돌아감.
            ui.RelaseTarget(COMMAND_INDEX);
            StartCoroutine("SkillAndItemCommandDown", ACTION_PICK_COMMAND);
        }

        /// <summary>
        /// inputManager의 입력에 따른 타겟 선택 커맨드 (이동)
        /// </summary>
        /// <param name="inputDir">입력</param>
        void TargetPickerMove(Vector3 inputDir)
        {
            // 선택된 스킬을 받아 옵니다.
            var skill = PREVCOMMAND_INDEX == SKILL_PICK_COMMAND
                ? playerUnits[_playerPickIndex].GetSkillData(_skillAndItemIndex)
                : Inventory.GetItemSkillData(_skillAndItemIndex);

            // 스킬의 타겟팅 범위
            SkillRange range = skill.range;
            // 스킬이 타겟이 아군인지 적군인지 파악
            bool ally = ((int)range & ALLYBITMASK) != 0;

            if (ally) // 아군을 타겟으로 하는 경우 이동
            {
                AddTargetPickIndex(-Mathf.RoundToInt(inputDir.x), true);
            }
            else // 적군을 타겟으로 하는 경우 이동

            {
                AddTargetPickIndex(Mathf.RoundToInt(inputDir.x)
                                   - Mathf.RoundToInt(inputDir.z), false);
            }

            // 선택된 타겟의 transform
            var target = ally ? playerUnits[_targetPickIndex].transform : enemyUnits[_targetPickIndex].transform;
            // 피커의 위치를 옮김
            SetPicker(true, enemyPicker, target.position + Vector3.up);
        }

        /// <summary>
        /// inputManager의 입력에 따른 타겟 선택 커맨드 (선택) 
        /// </summary>
        void TargetPickerSelection()
        {
            // 행동할 캐릭터
            var unit = playerUnits[_playerPickIndex];
            var isItem = PREVCOMMAND_INDEX == ITEM_PICK_COMMAND;
            // 행동할 스킬
            var skill = isItem ? Inventory.GetItemSkillData(_skillAndItemIndex) : unit.GetSkillData(_skillAndItemIndex);


            SkillRange range = skill.range;

            // 아군인지 적군인지 파악
            bool ally = ((int)range & ALLYBITMASK) != 0;

            // 타겟을 선택
            var targetUnit = ally ? playerUnits[_targetPickIndex] : enemyUnits[_targetPickIndex];

            // 정보를 바탕으로 캐릭터에 커맨드 입력
            BattleCommandSetting(unit.GetSkillData(_skillAndItemIndex),
                unit, isItem, targetUnit);
            // 다음 캐릭터 선택으로 넘어감
            SelectNextCharacter();

            // 다음 캐릭터 선택으로 넘어감
            void SelectNextCharacter()
            {
                // 타겟 선택 picker를 비활성화
                SetPicker(false, enemyPicker, Vector3.zero);

                int overFlow = AddPlayerPickIndex(1);
                if (overFlow == 1) // 다음 캐릭터가 없는 경우
                {
                    _waitCommand = false; // 해당 부분으로 인해서 사용자의 턴이 넘어가며
                    //  COMMAND_INDEX가 NONE_USE_PICK_COMMAND로 바뀜

                    // 활성화 중인 피커를 제거
                    SetPicker(false, playablePicker, Vector3.zero);
                    ui.RelaseTarget(PLAYER_PICK_COMMAND);
                    return;
                }

                // 다음 캐릭터를 선택
                SetPicker(true, playablePicker,
                    playerUnits[_playerPickIndex].transform.position + Vector3.up);
                ui.PickTarget(PLAYER_PICK_COMMAND, ui.charecterCards[_playerPickIndex]);
                // 커맨드를 행동 선택 커맨드로 변경
                COMMAND_INDEX = ACTION_PICK_COMMAND;
            }
        }

        /// <summary>
        /// inputManager의 입력에 따른 타겟 선택 커맨드 (취소)
        /// </summary>
        void TargetPickerCancel()
        {
            // 타겟 선택 picker를 비활성화
            SetPicker(false, enemyPicker, Vector3.zero);
            // 스킬/아이템 선택 커맨드로 돌아감
            // 이전 커맨드 입력 정보를 바탕으로
            StartCoroutine("SkillAndItemCommandOn", PREVCOMMAND_INDEX);
        }

        #endregion

        #region UI Animation

        IEnumerator SetCommandBoxHeight_Coroutine(int direction)
        {
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 10)
            {
                height = direction > 0 ? t : 1 - t;
                yield return null;
            }

            if (direction < 0)
            {
                height = 0;
                ui.RelaseTarget(PLAYER_PICK_COMMAND);
                ui.RelaseTarget(ACTION_PICK_COMMAND);
                ui.RelaseTarget(SKILL_PICK_COMMAND);
                yield break;
            }

            height = 1;
        }

        IEnumerator SkillAndItemCommandOn(int endCommand)
        {
            COMMAND_INDEX = NONE_USE_PICK_COMMAND;
            SetStartViewSkillAndItemIndex(0, endCommand);
            StopCoroutine("SkillAndItemCommandDown");
            yield return ui.SkillAndItemAreaMove(10);
            COMMAND_INDEX = endCommand;
            _skillAndItemIndex = 0;
            ui.PickTarget(endCommand,
                ui.skillAndItemCards[_skillAndItemIndex - _startViewSkillAndItemIndex].rectTransform);
        }

        IEnumerator SkillAndItemCommandDown(int endCommand)
        {
            COMMAND_INDEX = NONE_USE_PICK_COMMAND;
            ui.RelaseTarget(SKILL_PICK_COMMAND);
            StopCoroutine("SkillAndItemCommandOn");
            yield return ui.SkillAndItemAreaMove(-10);
            COMMAND_INDEX = endCommand;
        }

        IEnumerator CallPopup(float duration, string text, int endCommand)
        {
            COMMAND_INDEX = NONE_USE_PICK_COMMAND;
            yield return ui.ActivePopup(duration, text);
            COMMAND_INDEX = endCommand;
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            height = _height;
        }
#endif
    }
}