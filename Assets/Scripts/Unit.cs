using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static System.Math;

// изменение спрайта на смерть
// настройка других юнитов

namespace Assets.Scripts
{

    public abstract class Unit : MonoBehaviour
    {
        [SerializeField] int currentHp;
        [Space]
        public int MoveDistance;
        [SerializeField] int hp;
        [SerializeField] int hitDistance;
        [SerializeField] protected int damage;
        [SerializeField] protected int littleHitDistance = 1;
        [Space]
        [SerializeField] Camera camera;
        [SerializeField] NavMeshAgent navMeshAgent;
        [Space]
        [SerializeField] GameObject sprite;
        [SerializeField] GameObject spriteDeath;
        [SerializeField] GameObject unitTile;
        [SerializeField] Image hpImage;
        [SerializeField] Canvas statsCanvas;

        public bool IsDead => _currentHp <= 0;
        public IEnumerator WaitForEndTurn => new WaitUntil(() => _isEndOfUnitTurn);

        private bool _onFlipRight = true;
        private bool _isEndOfMove = true;
        private bool _isEndOfAttack = true;
        private bool _isEndOfUnitTurn = true;
        private Player _player;
        private Map _map;
        private int _currentHp;
        private int _maxHp;

        private void Update()
        {
            LookAtCamera();
            currentHp = _currentHp;

            if (!_isEndOfUnitTurn && Input.GetMouseButtonDown(0))
            {
                RaycastHit hit;

                // Проверка коснулась ли мышь кликом объекта
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                {
                    Transform targetPosition = StandOnCell(hit.transform);
                    Unit unit = ReturnUnit(targetPosition);

                    if (unit == null)
                        Move(targetPosition);
                    else
                        Attack(unit);
                }
            }
        }

        public virtual void Init(Player player, Map map)
        {
            LookAtCamera();

            _player = player;
            _map = map;

            _onFlipRight = player.Id == 1;
            if (!_onFlipRight)
            {
                FlipOnX();
                _onFlipRight = !_onFlipRight;
            }
            _maxHp = hp;
            _currentHp = hp;
            currentHp = _currentHp;

            OnInit();
        }

        /// <summary>
        /// Разворачивает все плоские спрайты перпендикулярно камере
        /// </summary>

        private void LookAtCamera()
        {
            sprite.transform.LookAt(transform.position - camera.transform.position, camera.transform.up);
            spriteDeath.transform.LookAt(transform.position - camera.transform.position, camera.transform.up);
            statsCanvas.transform.LookAt(transform.position - camera.transform.position, camera.transform.up);
            unitTile.transform.eulerAngles = Vector3.zero;
        }

        public void SetTurn()
        {
            ShineUnitTile();
            _isEndOfMove = false;
            _isEndOfAttack = false;
            _isEndOfUnitTurn = false;
        }

        /// <summary>
        /// Возвращает юнит в точке
        /// </summary>
        /// <param name="point">Метоположение клетки</param>
        /// <returns> Юнит, стоящий на клетке </returns>
        public Unit ReturnUnit(Transform point)
        {
            return _map.Units.FirstOrDefault(u => Round(Abs(u.transform.position.x - point.position.x)) == 0
                                            && Round(Abs(u.transform.position.z - point.position.z)) == 0);
        }

        /// <summary>
        /// Переводит позицию клика к центру клетки
        /// </summary>
        /// <param name="cellTransform">Место клика</param>
        /// <returns>Место центра</returns>
        private Transform StandOnCell(Transform cellTransform)
        {
            int x = (int)Round(cellTransform.position.x);
            int z = (int)Round(cellTransform.position.z);
            cellTransform.position = new Vector3(x, 0, z);
            return cellTransform;
        }

        /// <summary>
        /// Поворачиват юнита в сторону его движения
        /// </summary>
        private void FlipOnX()
        {
            SpriteRenderer plane = GetComponentInChildren<SpriteRenderer>();
            plane.flipX = !plane.flipX;
            _onFlipRight = !_onFlipRight;
        }

        /// <summary>
        /// Проверяет, возможно ли встать на клетку
        /// </summary>
        /// <param name="target">Клетка назначения</param>
        /// <returns>Можем ли двигаться</returns>
        private bool CanMove(Transform target)
        {
            if (Round(Abs(transform.position.x - target.position.x)) > MoveDistance || (Round(Abs(transform.position.z - target.position.z)) > MoveDistance))
                return false;

            if (ReturnUnit(target) != null)
                return false;

            if (_isEndOfMove)
                return false;

            foreach (Water w in _map.water)
                if (Round(w.transform.position.x) == Round(target.position.x) && Round(w.transform.position.z) == Round(target.position.z))
                    return false;

            return true;
        }

        /// <summary>
        /// Двигает юнит
        /// </summary>
        /// <param name="targetPosition">Клетка назначения</param>
        private void Move(Transform targetPosition)
        {
            if (!CanMove(targetPosition))
                return;

            if (_onFlipRight && targetPosition.position.x < transform.position.x || (!_onFlipRight) && targetPosition.position.x > transform.position.x)
                FlipOnX();

            navMeshAgent.destination = targetPosition.position;
            _isEndOfMove = true;
            _map.HideDistanceGrass();

            if (_isEndOfAttack)
                EndTurn();
        }

        /// <summary>
        /// Проверяет, можем ли ударить
        /// </summary>
        /// <param name="attackedUnit">Атакуемый юнит</param>
        /// <returns>Можем лм атаковать</returns>
        private bool CanAttack(Unit attackedUnit)
        {
            var attackedCoordinates = StandOnCell(attackedUnit.transform).position;
            var attackingCoordinates = StandOnCell(transform).position;

            if (attackedUnit.IsDead)
                return false;

            if (attackedUnit._player == _player)
                return false;

            if (_isEndOfAttack)
                return false;

            return Round(Abs(attackingCoordinates.x - attackedCoordinates.x)) <= hitDistance &&
                    Round(Abs(attackingCoordinates.z - attackedCoordinates.z)) <= hitDistance;

        }

        /// <summary>
        /// наносим урон юниту
        /// </summary>
        /// <param name="attackedUnit">Атакуемый юнит</param>
        private void Attack(Unit attackedUnit)
        {
            if (!CanAttack(attackedUnit))
                return;

            int damage = OnAttack(attackedUnit);
            attackedUnit.GetDamage(damage);

            _isEndOfAttack = true;

            if (_isEndOfMove)
            {
                EndTurn();
            }
        }

        /// <summary>
        /// Уменьшает hp юнита
        /// </summary>
        /// <param name="damage">Количество урона</param>
        private void GetDamage(int damage)
        {
            _currentHp = Max(_currentHp - damage, 0);
            hpImage.fillAmount = Max((float)_currentHp / _maxHp, 0);

            if (IsDead)
            {
                Death();
            }
        }

        /// <summary>
        /// Включает визуал для хода юнита
        /// </summary>
        private void ShineUnitTile()
        {
            unitTile.SetActive(true);
            _map.ShineDistanceGrass(this);
        }

        /// <summary>
        /// Выключает визуал после хода юнита
        /// </summary>
        private void HideUnitTile()
        {
            unitTile.SetActive(false);
            _map.HideDistanceGrass();
        }

        /// <summary>
        /// Заканчивает ход, выключает визуал
        /// </summary>
        public void EndTurn()
        {
            _isEndOfUnitTurn = true;
            HideUnitTile();
        }

        /// <summary>
        /// Включает визуал смерти
        /// </summary>
        private void Death()
        {
            sprite.SetActive(false);
            spriteDeath.SetActive(true);
        }

        protected abstract void OnInit();

        protected abstract int OnAttack(Unit attackedUnit);
    }

}
