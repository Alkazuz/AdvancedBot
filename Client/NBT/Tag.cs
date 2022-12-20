/*
 * Original: https://mojang.com/2012/02/new-minecraft-map-format-anvil
 */

using System.Text;
using System;
namespace AdvancedBot.client.NBT
{
    /**
     * Copyright Mojang AB.
     * 
     * Don't do evil.
     */
    public abstract class Tag
    {
        public const byte TAG_End = 0;
        public const byte TAG_Byte = 1;
        public const byte TAG_Short = 2;
        public const byte TAG_Int = 3;
        public const byte TAG_Long = 4;
        public const byte TAG_Float = 5;
        public const byte TAG_Double = 6;
        public const byte TAG_Byte_Array = 7;
        public const byte TAG_String = 8;
        public const byte TAG_List = 9;
        public const byte TAG_Compound = 10;
        public const byte TAG_Int_Array = 11;

        private string name;

        public abstract void Write(DataOutput dos);
        public abstract void Load(DataInput dis);

        public abstract byte GetID();

        protected Tag(string name)
        {
            if (name == null)
                this.name = "";
            else
                this.name = name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Tag))
                return false;
            Tag o = (Tag)obj;

            if (GetID() != o.GetID()) return false;
            if (name == null && o.name != null || name != null && o.name == null) return false;
            if (name != null && !name.Equals(o.name)) return false;
            return true;
        }

        public string Print()
        {
            StringBuilder sb = new StringBuilder();
            print("", sb);
            return sb.ToString();
        }

        public virtual void print(string prefix, StringBuilder sb)
        {
            string name = GetName();

            sb.Append(prefix);
            sb.Append(GetTagName(GetID()));
            if (name.Length > 0) {
                sb.Append("(\"" + name + "\")");
            }
            sb.Append(": ");
            sb.AppendLine(ToString());
        }

        public Tag SetName(string name)
        {
            if (name == null)
                this.name = "";
            else
                this.name = name;
            return this;
        }
        public string GetName()
        {
            if (name == null) return "";
            return name;
        }

        public static Tag ReadNamedTag(DataInput dis)
        {
            byte type = dis.ReadByte();
            if (type == 0) return new EndTag();

            string name = dis.ReadUTF();// new string(bytes, "UTF-8");
            Tag tag = NewTag(type, name);
            //        short length = dis.readShort();
            //        byte[] bytes = new byte[length];
            //        dis.readFully(bytes);

            tag.Load(dis);
            return tag;
        }
        public static void WriteNamedTag(Tag tag, DataOutput dos)
        {
            dos.WriteByte(tag.GetID());
            if (tag.GetID() == Tag.TAG_End) return;

            //        byte[] bytes = tag.getName().getBytes("UTF-8");
            //        dos.writeShort(bytes.length);
            //        dos.write(bytes);
            dos.WriteUTF(tag.GetName());

            tag.Write(dos);
        }

        public static Tag NewTag(byte type, string name)
        {
            switch (type)
            {
                case TAG_End: return new EndTag();
                case TAG_Byte: return new ByteTag(name);
                case TAG_Short: return new ShortTag(name);
                case TAG_Int: return new IntTag(name);
                case TAG_Long: return new LongTag(name);
                case TAG_Float: return new FloatTag(name);
                case TAG_Double: return new DoubleTag(name);
                case TAG_Byte_Array: return new ByteArrayTag(name);
                case TAG_Int_Array: return new IntArrayTag(name);
                case TAG_String: return new StringTag(name);
                case TAG_List: return new ListTag<Tag>(name);
                case TAG_Compound: return new CompoundTag(name);
            }
            return null;
        }
        public static string GetTagName(byte type)
        {
            switch (type)
            {
                case TAG_End: return "TAG_End";
                case TAG_Byte: return "TAG_Byte";
                case TAG_Short: return "TAG_Short";
                case TAG_Int: return "TAG_Int";
                case TAG_Long: return "TAG_Long";
                case TAG_Float: return "TAG_Float";
                case TAG_Double: return "TAG_Double";
                case TAG_Byte_Array: return "TAG_Byte_Array";
                case TAG_Int_Array: return "TAG_Int_Array";
                case TAG_String: return "TAG_String";
                case TAG_List: return "TAG_List";
                case TAG_Compound: return "TAG_Compound";
            }
            return "UNKNOWN";
        }
        public static byte GetTagType(Type type)
        {
            switch (type.Name)
            {
                case "EndTag": return TAG_End;
                case "ByteTag": return TAG_Byte;
                case "ShortTag": return TAG_Short;
                case "IntTag": return TAG_Int;
                case "LongTag": return TAG_Long;
                case "FloatTag:": return TAG_Float;
                case "DoubleTag": return TAG_Double;
                case "ByteArrayTag": return TAG_Byte_Array;
                case "IntArrayTag": return TAG_Int_Array;
                case "StringTag": return TAG_String;
                case "ListTag": return TAG_List;
                case "CompoundTag": return TAG_Compound;
            }
            return TAG_End;
        }

        public override int GetHashCode()
        {
            return GetID() ^ GetName().GetHashCode();
        }

        public abstract Tag Copy();
    }
}