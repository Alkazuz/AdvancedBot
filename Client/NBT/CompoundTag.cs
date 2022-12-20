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

    public class CompoundTag : Tag, IEnumerable
    {
        private Dictionary<string, Tag> tags = new Dictionary<string, Tag>();

        public CompoundTag()
            : base("")
        {
        }

        public CompoundTag(string name)
            : base(name)
        {
        }

        public override void Write(DataOutput dos)
        {
            foreach (Tag tag in tags.Values) {
                Tag.WriteNamedTag(tag, dos);
            }
            dos.WriteByte(Tag.TAG_End);
        }
        public override void Load(DataInput dis)
        {
            tags.Clear();
            Tag tag;
            while ((tag = Tag.ReadNamedTag(dis)).GetID() != Tag.TAG_End) {
                tags[tag.GetName()] = tag;
            }
        }

        public List<Tag> GetAllTags()
        {
            return new List<Tag>(tags.Values);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return tags.Values.GetEnumerator();
        }

        public bool Remove(string name)
        {
            return tags.Remove(name);
        }

        public override byte GetID()
        {
            return TAG_Compound;
        }

        public CompoundTag Add(string name, Tag tag)
        {
            tags[name] = tag.SetName(name);
            return this;
        }
        public void AddByte(string name, byte value)
        {
            tags[name] = new ByteTag(name, value);
        }
        public void AddShort(string name, short value)
        {
            tags[name] = new ShortTag(name, value);
        }
        public void AddInt(string name, int value)
        {
            tags[name] = new IntTag(name, value);
        }
        public void AddLong(string name, long value)
        {
            tags[name] = new LongTag(name, value);
        }
        public void AddFloat(string name, float value)
        {
            tags[name] = new FloatTag(name, value);
        }
        public void AddDouble(string name, double value)
        {
            tags[name] = new DoubleTag(name, value);
        }
        public void AddString(string name, string value)
        {
            tags[name] = new StringTag(name, value);
        }
        public void AddByteArray(string name, byte[] value)
        {
            tags[name] = new ByteArrayTag(name, value);
        }
        public void AddIntArray(string name, int[] value)
        {
            tags[name] = new IntArrayTag(name, value);
        }
        public void AddCompound(string name, CompoundTag value)
        {
            tags[name] = value.SetName(name);
        }
        public void AddBoolean(string name, bool val)
        {
            AddByte(name, (byte)(val ? 1 : 0));
        }


        public Tag Get(string name)
        {
            Tag t;
            tags.TryGetValue(name, out t);
            return t;
        }
        public bool Contains(string name)
        {
            return tags.ContainsKey(name);
        }

        public byte GetByte(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return ((ByteTag)t).Data;
            }
            return 0;
        }
        public short GetShort(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return ((ShortTag)t).Data;
            }
            return 0;
        }
        public int GetInt(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return ((IntTag)t).Data;
            }
            return 0;
        }
        public long GetLong(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return ((LongTag)t).Data;
            }
            return 0;
        }
        public float GetFloat(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return ((FloatTag)t).Data;
            }
            return 0;
        }
        public double GetDouble(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return ((DoubleTag)t).Data;
            }
            return 0;
        }

        public string GetStringOrDefault(string name, string defuat)
        {
            try
            {
                Tag t;
                if (tags.TryGetValue(name, out t))
                {
                    return ((StringTag)t).Data;
                }
            }
            catch (Exception ex) { }
            
            return defuat;
        }

        public string GetString(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return ((StringTag)t).Data;
            }
            return "";
        }
        public byte[] GetByteArray(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return ((ByteArrayTag)t).Data;
            }
            return new byte[0];
        }
        public int[] GetIntArray(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return ((IntArrayTag)t).Data;
            }
            return new int[0];
        }
        public CompoundTag GetCompound(string name)
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return (CompoundTag)t;
            }
            return new CompoundTag(name);
        }
        public ListTag<Tag> GetList(string name)//public ListTag<T> GetList<T>(string name) where T : Tag
        {
            Tag t;
            if (tags.TryGetValue(name, out t)) {
                return (ListTag<Tag>)t;
            }
            return new ListTag<Tag>(name);
        }
        public bool GetBoolean(string name)
        {
            return GetByte(name) != 0;
        }

        public bool GetBoolOrTrue(string name)
        {
            Tag tag;
            if (tags.TryGetValue(name, out tag))
                return ((ByteTag)tag).Data != 0;
            return true;
        }
        public int GetIntOrDefault(string name, int _default)
        {
            Tag tag;
            if (tags.TryGetValue(name, out tag))
                return ((IntTag)tag).Data;
            return _default;
        }
        public double GetDoubleOrDefault(string name, double _default)
        {
            Tag tag;
            if (tags.TryGetValue(name, out tag))
                return ((DoubleTag)tag).Data;
            return _default;
        }

        public override string ToString()
        {
            return "" + tags.Count + " entries";
        }
        public override void print(string prefix, StringBuilder sb)
        {
            base.print(prefix, sb);
            sb.AppendLine(prefix + "{");
            string orgPrefix = prefix;
            prefix += "   ";
            foreach (Tag tag in tags.Values) {
                tag.print(prefix, sb);
            }
            sb.AppendLine(orgPrefix + "}");
        }
        public bool IsEmpty()
        {
            return tags.Count == 0;
        }

        public override Tag Copy()
        {
            CompoundTag tag = new CompoundTag(GetName());
            /*foreach(string key in tags.Keys) {
                tag.put(key, tags[key].copy());
            }*/
            foreach (KeyValuePair<string, Tag> kv in tags)
                tag.Add(kv.Key, kv.Value.Copy());
            return tag;
        }
        public override bool Equals(object obj)
        {
            if (base.Equals(obj)) {
                CompoundTag o = (CompoundTag)obj;
                //return tags.Values.Equals(o.tags.Values);
                foreach (KeyValuePair<string, Tag> tag in tags) {
                    Tag t2;
                    if (!o.tags.TryGetValue(tag.Key, out t2)) return false;
                    if (!t2.Equals(tag.Value)) return false;
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ tags.GetHashCode();
                hash = (hash * 16777619) ^ GetID().GetHashCode();
                hash = (hash * 16777619) ^ GetName().GetHashCode();
                return hash;
            }
        }
    }
}