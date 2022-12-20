﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvancedBot.Json
{
    public class With
    {
        private ClickEvent clickEvent;
        private HoverEvent hoverEvent;
        private String text;

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

        public String getText()
        {
            return text;
        }

        public void setText(String text)
        {
            this.text = text;
        }

        public ChatMessage getNonNull(ChatMessage mes)
        {
            if (this.text != null) mes.setText(this.text);
            //        if(this.translate != null)mes.setTranslate(this.translate);
            //        if(this.score != null)mes.setScore(this.score);
            //        if(this.selector != null)mes.setSelector(this.selector);
            //        if(this.extra != null)mes.setExtra(this.extra);
            //        if(this.bold != null)mes.setBold(this.bold);
            //        if(this.italic != null)mes.setItalic(this.italic);
            //        if(this.underlined != null)mes.setUnderlined(this.underlined);
            //        if(this.strikethrough != null)mes.setStrikethrough(this.strikethrough);
            //        if(this.obfuscated != null)mes.setObfuscated(this.obfuscated);
            //        if(this.color != null)mes.setColor(this.color);
            if (this.clickEvent != null) mes.setClickEvent(this.clickEvent);
            if (this.hoverEvent != null) mes.setHoverEvent(this.hoverEvent);
            return mes;
        }

    }
}
