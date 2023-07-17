using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Assets.Scripts
{
    public class Player : MonoBehaviour
    {
        public int Id;
        public string Name;

        [SerializeField] Unit[] units;

        public Unit[] Units => units;

        public void Init(int id, string name, Map map)
        {
            Id = id;
            Name = name;

            foreach (var u in units)
            {
                u.Init(this, map);
            }
        }

        /// <summary>
        /// Задаем очередь хода игрока, пока не сходят все его юниты, метод не закончится
        /// </summary>
        /// <returns></returns>
        public IEnumerator SetTurn()
        {
            foreach (var u in units.Where(u => !u.IsDead))
            {
                u.SetTurn();
                yield return u.WaitForEndTurn;
            }
        }


        /// <summary>
        /// Пропускаем ход текущего юнита
        /// </summary>
        public void SkipTurn()
        {
            foreach (var u in units)
            {
                u.EndTurn();
            }
        }
    }
}
