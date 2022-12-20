using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Json
{
    public class HoverEvent
    {
        private String action;
        private Value value;

        public String getAction()
        {
            return action;
        }

        public void setAction(String action)
        {
            this.action = action;
        }

        public Value getValue()
        {
            return value;
        }

        public void setValue(Value value)
        {
            this.value = value;
        }

        internal void setAction(object p)
        {
            throw new NotImplementedException();
        }
    }
}
