using System.Collections.Generic;
using UnityEngine;

namespace Scipts.Editor
{
    [CreateAssetMenu(fileName = "Level Paint Data", menuName = "Level Paint/Paint Data", order = 0)]
    public class LevelPaintData : ScriptableObject
    {
        [SerializeField]
        public List<GameObject> PaintObjects;
    }
}