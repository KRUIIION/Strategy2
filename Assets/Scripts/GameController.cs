using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Assets.Scripts
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] Map map;
        [SerializeField] Player player1;
        [SerializeField] Player player2;


        private void Start()
        {
            player1.Init(1, "KAdwk", map);
            player2.Init(2, "AKWDlk", map);

            List<Unit> units = new List<Unit>();
            units.AddRange(player1.Units);
            units.AddRange(player2.Units);

            map.Init(units.ToArray());

            StartCoroutine(StepsForArmy());
        }

        /// <summary>
        /// Задаем поочередные ходы игроков
        /// </summary>
        /// <returns></returns>
        IEnumerator StepsForArmy()
        {
            while (true)
            {
                yield return player1.SetTurn();
                yield return player2.SetTurn();
            }
        }
    }
}
