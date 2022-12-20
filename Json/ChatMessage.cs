﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Json
{
    public class ChatMessage
    {
        private String text = "";
        private String translate;
        private List<With> with = new List<With>();
        private String score;
        private String selector;
        private List<Object> extra = new List<Object>();
        private String bold = "false";
        private String italic = "false";
        private String underlined = "false";
        private String strikethrough = "false";
        private String obfuscated = "false";
        private String color = "black";
        private ClickEvent clickEvent;
        private HoverEvent hoverEvent;
        private String insertion;

        public String getText()
        {
            return text;
        }
        public void setText(String text)
        {
            this.text = text;
        }
        public String getTranslate()
        {
            return translate;
        }
        public void setTranslate(String translate)
        {
            this.translate = translate;
        }
        public List<With> GetWith()
        {
            return with;
        }
        public void SetWith(List<With> with)
        {
            this.with = with;
        }
        public String getScore()
        {
            return score;
        }
        public void setScore(String score)
        {
            this.score = score;
        }
        public String getSelector()
        {
            return selector;
        }
        public void setSelector(String selector)
        {
            this.selector = selector;
        }
        public List<Object> getExtra()
        {
            return extra;
        }
        public void setExtra(List<Object> extra)
        {
            this.extra = extra;
        }
        public String getBold()
        {
            return bold;
        }
        public void setBold(String bold)
        {
            this.bold = bold;
        }
        public String getItalic()
        {
            return italic;
        }
        public void setItalic(String italic)
        {
            this.italic = italic;
        }
        public String getUnderlined()
        {
            return underlined;
        }
        public void setUnderlined(String underlined)
        {
            this.underlined = underlined;
        }
        public String getStrikethrough()
        {
            return strikethrough;
        }
        public void setStrikethrough(String strikethrough)
        {
            this.strikethrough = strikethrough;
        }
        public String getObfuscated()
        {
            return obfuscated;
        }
        public void setObfuscated(String obfuscated)
        {
            this.obfuscated = obfuscated;
        }
        public String getColor()
        {
            return color;
        }
        public void setColor(String color)
        {
            this.color = color;
        }
        public ClickEvent getClickEvent()
        {
            return clickEvent;
        }
        public void setClickEvent(ClickEvent clickEvent)
        {
            this.clickEvent = clickEvent;
        }
        public HoverEvent getHoverEvent()
        {
            return hoverEvent;
        }
        public void setHoverEvent(HoverEvent hoverEvent)
        {
            this.hoverEvent = hoverEvent;
        }
        public String getInsertion()
        {
            return insertion;
        }
        public void setInsertion(String insertion)
        {
            this.insertion = insertion;
        }

    }

    class Clicked
    {
        private String action;
        private String value;

        public String getAction()
        {
            return action;
        }
        public void setAction(String action)
        {
            this.action = action;
        }
        public String getValue()
        {
            return value;
        }
        public void setValue(String value)
        {
            this.value = value;
        }
    }

    class Hover
    {
        private String action;
        private String value;

        public String getAction()
        {
            return action;
        }
        public void setAction(String action)
        {
            this.action = action;
        }
        public String getValue()
        {
            return value;
        }
        public void setValue(String value)
        {
            this.value = value;
        }
    }
}
