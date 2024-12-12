using ChromaCore.Code.Objects;
using ChromaCore.Code.Scenes;

namespace ChromaCore.Code.Utils.Input
{
    public class TrainingDummyController : Controller
    {
        protected Fighter player;
        protected Fighter target;

        public TrainingDummyActions action = TrainingDummyActions.Stand;
        public TrainingDummyBlockTypes blockMode;

        public TrainingDummyController(Fighter player) : base()
        {
            this.player = player;
        }

        public override void UpdateKeys(int direction)
        {
            target = player.nearestPlayer;

            //Reset inputs
            for (int i = 0; i < keyPressed.Length; i++)
            {
                if (Game.Instance.Scene is InGame scene)
                {
                    if (scene.hitpause <= 0)
                    {
                        if (keyPressed[i] > 0) keyPressed[i]--;
                        if (keyReleased[i] > 0) keyReleased[i]--;
                    }
                }
                keyDown[i] = false;
            }

            if (action == TrainingDummyActions.Crouch) keyDown[Key_Down] = true;
            if (action == TrainingDummyActions.ShortHop && player.Grounded && !player.CommitedState) keyPressed[Key_Up] = Inputbuffer;
            if (action == TrainingDummyActions.FullHop && player.Grounded)
            {
                if (!player.CommitedState) keyPressed[Key_Up] = Inputbuffer;
                keyDown[Key_Up] = true;
            }
            UpdateMotionInputs(direction);
        }
    }

    public enum TrainingDummyActions
    {
        Stand,
        Crouch,
        ShortHop,
        FullHop,
        Block,
        EnumLength
    }

    public enum TrainingDummyBlockTypes
    {
        All,
        High,
        Low,
        EnumLength
    }
}
