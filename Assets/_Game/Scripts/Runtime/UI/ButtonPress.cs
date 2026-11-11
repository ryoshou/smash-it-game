using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>Sinks a button's face onto its lip while it is held, so a press feels physical.</summary>
    public sealed class ButtonPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float SinkShare = 0.7f;

        [SerializeField] private RectTransform face;
        [SerializeField] private Shadow lip;

        private Vector2 restOffsetMin;
        private Vector2 restOffsetMax;
        private Vector2 restLip;
        private bool isDown;

        private void Awake()
        {
            restOffsetMin = face.offsetMin;
            restOffsetMax = face.offsetMax;
            restLip = lip.effectDistance;
        }

        private void OnDisable() => Release();

        public void OnPointerDown(PointerEventData eventData)
        {
            if (isDown) return;
            isDown = true;
            var sink = new Vector2(0f, restLip.y * SinkShare);
            face.offsetMin = restOffsetMin + sink;
            face.offsetMax = restOffsetMax + sink;
            lip.effectDistance = restLip - sink;
        }

        public void OnPointerUp(PointerEventData eventData) => Release();

        public void OnPointerExit(PointerEventData eventData) => Release();

        private void Release()
        {
            if (!isDown) return;
            isDown = false;
            face.offsetMin = restOffsetMin;
            face.offsetMax = restOffsetMax;
            lip.effectDistance = restLip;
        }
    }
}
