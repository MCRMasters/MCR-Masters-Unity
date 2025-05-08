using Newtonsoft.Json.Linq;
using UnityEngine;
using MCRGame.Common;

namespace MCRGame.Game.Events
{
    public class FlowerHandler : MonoBehaviour, IGameEventHandler
    {
        public void Handle(GameEventType evt, JObject data)
        {
            if (evt != GameEventType.FLOWER) return;

            if (HandlerUtil.GM.IsMyTurn)
                HandlerUtil.GM.IsFlowerConfirming = true;
            HandlerUtil.GM.StartCoroutine(HandlerUtil.GM.ConfirmFlower(data));
        }
    }
}
