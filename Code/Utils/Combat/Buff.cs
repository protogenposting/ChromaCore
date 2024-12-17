using ChromaCore.Code.Objects;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChromaCore.Code.Utils.Combat
{
    public class Buff
    {
        public string identifier = "";
        public int duration = 0;
        public Action<Buff, Fighter> onTick = null;
        public Action<SpriteBatch, Buff, Fighter> onDraw = null;

        Buff(string id, int dur, Action<Buff, Fighter> tick)
        {
            identifier = id;
            duration = dur;
            onTick = tick;
        }

        public static Buff Apply(Fighter target, string identifier, int duration, Action<Buff, Fighter> tickEffect = null, Action<SpriteBatch, Buff, Fighter> drawEffect = null)
        {
            foreach (Buff b2 in target.buffs.ToArray()) if (b2.identifier == identifier) target.buffs.Remove(b2);
            Buff b = new Buff(identifier, duration, tickEffect) { onDraw = drawEffect };
            target.buffs.Add(b);
            return b;
        }

        public Buff Clone()
        {
            Buff b = new Buff(identifier, duration, null);
            b.onTick = onTick;
            b.onDraw = onDraw;
            return b;
        }

        public void Update(Fighter player)
        {
            duration--;
            onTick?.Invoke(this, player);
            if (duration <= 0)
            {
                player.buffs.Remove(this);
            }
        }
    }
}
