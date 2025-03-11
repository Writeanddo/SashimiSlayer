using System.Collections.Generic;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Menus.StartMenu
{
    public class StartMenuFishSlice : MonoBehaviour
    {
        [BoldHeader("Start Menu Fish Slice Minigame")]
        [InfoBox("Handles the fish slicing minigame")]
        [Header("Depends")]

        [SerializeField]
        private List<StartMenuFish> _fish;

        [SerializeField]
        private Transform _fishPlaceTransform;

        [Header("Events")]

        [SerializeField]
        private UnityEvent _onFishSlice;

        [SerializeField]
        private UnityEvent _onFishAttemptSlice;

        [SerializeField]
        private UnityEvent _onFishPlace;

        [SerializeField]
        private UnityEvent _onFishAttemptPlace;

        private StartMenuFish _currentFish;

        private void Start()
        {
            PlaceFish();
        }

        public void PlaceFish()
        {
            if (_currentFish != null)
            {
                _onFishAttemptPlace.Invoke();
                return;
            }

            // Place a new fish down
            StartMenuFish fishPrefab = _fish[Random.Range(0, _fish.Count)];
            StartMenuFish newFish = Instantiate(fishPrefab, _fishPlaceTransform.position, Quaternion.identity);
            _currentFish = newFish;
            _currentFish.Place();
            _onFishPlace.Invoke();
        }

        public void SliceFish()
        {
            if (_currentFish == null)
            {
                _onFishAttemptSlice.Invoke();
                return;
            }

            _currentFish.Slice();
            _currentFish = null;
            _onFishSlice.Invoke();
        }
    }
}