using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Json
{
    class Node
    {
        private String color;
        private String text;

        public String getColor()
        {
            return color;
        }
        public void setColor(String color)
        {
            this.color = color;
        }
        public String getText()
        {
            return text;
        }
        public void setText(String text)
        {
            this.text = text;
        }
    }
}
