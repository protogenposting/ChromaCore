using Microsoft.Xna.Framework.Input;
using RCArena.Code.Objects;
using RCArena.Code.Scenes;

namespace RCArena.Code.Utils.Input
{
    public class TrainingDummyController : Controller
    {
        protected Fighter player;
        protected Fighter target;

        public TrainingDummyActions action = TrainingDummyActions.Stand;
        public TrainingDummyBlockTypes blockMode;

        public TrainingDummyController(Fighter player) : base(-2)
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
            if (action == TrainingDummyActions.Block && target != null)
            {
                if (blockMode == TrainingDummyBlockTypes.Low) keyDown[Key_Down] = true;
                if (target.state == Fighter.States.Attack && target.attack.hitboxes != null)
                {
                    HitboxSpawner hitbox = target.attack.hitboxes.FirstOrDefault(h => h.creationFrame >= target.attackTimer);
                    int key = target.position.X > player.position.X ? Key_Left : Key_Right;
                    if (hitbox != null && target.attackTimer >= hitbox.creationFrame - 1)
                    {
                        keyDown[key] = true;
                        if (hitbox.hitType == HitTypes.Low && blockMode != TrainingDummyBlockTypes.High) keyDown[Key_Down] = true;
                    }
                }

                if (player.state == Fighter.States.LowGuard) keyDown[Key_Down] = true;
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
