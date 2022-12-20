using System.Collections.Generic;
using System.Text;
using System.Collections;
using System;
namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */
    public class ListTag<T> : Tag, IEnumerable where T : Tag
    {
        private List<T> list = new List<T>();
        private byte type;

        public int Count => list.Count;
        public T this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        public ListTag()
            : base("")
        {
            type = GetTagType(typeof(T));
            if (type == TAG_End) type = TAG_Byte;
        }

        public ListTag(string name)
            : base(name)
        {
            type = GetTagType(typeof(T));
            if (type == TAG_End) type = TAG_Byte;
        }

        public override void Write(DataOutput dos)
        {
            dos.WriteByte(type);
            dos.WriteInt(list.Count);
            for (int i = 0; i < list.Count; i++)
                list[i].Write(dos);
        }
        public override void Load(DataInput dis)
        {
            type = dis.ReadByte();
            int size = dis.ReadInt();

            list = new List<T>(size);
            for (int i = 0; i < size; i++) {
                Tag tag = Tag.NewTag(type, null);
                tag.Load(dis);
                list.Add((T)tag);
            }
        }

        public override byte GetID()
        {
            return TAG_List;
        }

        public override string ToString()
        {
            return "" + list.Count + " entries of type " + Tag.GetTagName(type);
        }

        public override void print(string prefix, StringBuilder sb)
        {
            base.print(prefix, sb);

            sb.AppendLine(prefix + "{");
            string orgPrefix = prefix;
            prefix += "   ";
            for (int i = 0; i < list.Count; i++)
                list[i].print(prefix, sb);
            sb.AppendLine(orgPrefix + "}");
        }

        public void AddTag(T tag)
        {
            type = tag.GetID();
            list.Add(tag);
        }

        public T GetTag(int index)
        {
            return list[index];
        }

        public override Tag Copy()
        {
            ListTag<T> res = new ListTag<T>(GetName());
            res.type = type;
            foreach (T t in list)
            {
                T copy = (T)t.Copy();
                res.list.Add(copy);
            }
            return res;
        }

        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }
        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }
        
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                ListTag<T> o = (ListTag<T>)obj;
                if (type == o.type)
                {
                    return list.Equals(o.list);
                }
            }
            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ list.GetHashCode();
                hash = (hash * 16777619) ^ GetID().GetHashCode();
                hash = (hash * 16777619) ^ GetName().GetHashCode();
                return hash;
            }
        }
    }
}