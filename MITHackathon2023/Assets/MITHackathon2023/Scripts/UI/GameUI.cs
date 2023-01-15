using MITHack.Robot.Game;
using UnityEngine;
using UnityEngine.UI;

namespace MITHack.Robot.Utils.UI
{
    public class GameUI : MonoBehaviour
    {
        [Header("Chicken Nuggets")] 
        [SerializeField]
        private Image counterImage;
        [SerializeField]
        private TMPro.TMP_Text counterText;

        private void Update()
        {
            var gameManager = GameManager.Get();
            if (gameManager)
            {
                var chickensKilled = gameManager.ChickensKilled;
                if (counterText)
                {
                    counterText.enabled = chickensKilled > 0;
                    counterText.text = $"x{gameManager.ChickensKilled}";
                }

                if (counterImage)
                {
                    counterImage.enabled = chickensKilled > 0;
                }
            }
        }
    }
}