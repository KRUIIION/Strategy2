using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static System.Math;


namespace Assets.Scripts
{
    public class Map : MonoBehaviour
    {
        public Unit[] Units { get; private set; }

        [SerializeField] public Grass[] grass;
        [SerializeField] public Water[] water;
        [SerializeField] GameObject selectPlane;

        private GameObject currentPlane;
        private int x;
        private int z;

        public void Init(Unit[] units)
        {
            Units = units;
        }

        private void Update()
        {
            CursorShineGrass();
        }

        /// <summary>
        /// Подсвечивает клетки под мышкой
        /// </summary>
        void CursorShineGrass()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 point = hit.point;

                if (!(x == Round(point.x) && z == Round(point.z)))
                {
                    Destroy(currentPlane);
                    x = (int)Round(point.x);
                    z = (int)Round(point.z);

                    point = new Vector3(x, 0.1f, z);
                    currentPlane = Instantiate(selectPlane, point, Quaternion.identity, transform);
                    selectPlane.SetActive(true);
                }
            }
            else
            {
                selectPlane.SetActive(false);
            }
        }

        /// <summary>
        /// Подсвечивает клетки на которые можно сходить вокруг юнита
        /// </summary>
        /// <param name="unit">Юнит, для которого включаем визуал</param>
        public void ShineDistanceGrass(Unit unit)
        {
            foreach (var u in Units)
            {

                grass.Where(g => g != null)
                    .Where(g => Round(Abs(g.transform.position.x - unit.transform.position.x)) <= unit.MoveDistance && Round(Abs(g.transform.position.z - unit.transform.position.z)) <= unit.MoveDistance)
                    .Where(g => unit.ReturnUnit(g.transform) == null)
                    .ToList()
                    .ForEach(g => g.moveTile.SetActive(true));
            }
        }

        /// <summary>
        /// Скрываем подсвеченые клетки
        /// </summary>
        public void HideDistanceGrass()
        {
            grass.Where(g => g != null)
                .ToList()
                .ForEach(g => g.moveTile.SetActive(false));
        }
    }

    //public void ShineAttackGrass(Unit unit)
    //{
    //    grass.Where(g => g != null)
    //       .Where(g => Round(Abs(g.transform.position.x - unit.transform.position.x)) <= unit.hitDistance && Round(Abs(g.transform.position.z - unit.transform.position.z)) <= unit.hitDistance)
    //       .Where(g => unit.ReturnUnit(g.transform) != null) // чтобы светил только юнитов
    //       .Where(g => unit.ReturnUnit(g.transform)._player.Id != unit._player.Id)  // и врагов
    //       .Where(g => g.transform.position.x - unit.transform.position.x >= 1 && g.transform.position.x - unit.transform.position.x >= 1) // чтобы не светил нас
    //       .ToList()
    //       .ForEach(g => g.attackTile.SetActive(true));
    //}


    //public void HideAttackGrass()
    //{
    //    grass.Where(g => g != null)
    //        .ToList()
    //        .ForEach(g => g.attackTile.SetActive(false));
    //}
}
